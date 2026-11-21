using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Loader;

namespace Dsw2025Tpi.E2ETest.Helpers;

internal static class E2EApiHost
{
    private static readonly object LockObj = new();
    private static Process? _apiProcess;
    private static string? _baseUrl;

    static E2EApiHost()
    {
        AppDomain.CurrentDomain.ProcessExit += (_, _) => Stop();
        AssemblyLoadContext.Default.Unloading += _ => Stop();
    }

    public static string EnsureStarted()
    {
        lock (LockObj)
        {
            if (_apiProcess is { HasExited: false } && _baseUrl is not null)
                return _baseUrl;

            Stop();

            var apiProjectPath = FindApiProjectPath();
            var port = GetFreePort();
            var baseUrl = $"http://127.0.0.1:{port}";
            var logs = new ConcurrentQueue<string>();

            var startInfo = new ProcessStartInfo("dotnet")
            {
                Arguments = $"run --project \"{apiProjectPath}\" --no-launch-profile --urls {baseUrl}",
                WorkingDirectory = Path.GetDirectoryName(apiProjectPath)!,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";

            var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
            void CaptureLog(string? line)
            {
                if (string.IsNullOrWhiteSpace(line)) return;
                logs.Enqueue(line);
                while (logs.Count > 30) logs.TryDequeue(out _);
            }

            process.OutputDataReceived += (_, e) => CaptureLog(e.Data);
            process.ErrorDataReceived += (_, e) => CaptureLog(e.Data);

            if (!process.Start())
                throw new InvalidOperationException("No se pudo iniciar Dsw2025Tpi.Api para los tests E2E.");

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            try
            {
                WaitUntilReady(process, baseUrl, logs);
                _apiProcess = process;
                _baseUrl = baseUrl;
                return baseUrl;
            }
            catch
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                process.Dispose();
                throw;
            }
        }
    }

    private static void WaitUntilReady(Process process, string baseUrl, ConcurrentQueue<string> logs)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTime.UtcNow.AddSeconds(45);

        while (DateTime.UtcNow < deadline)
        {
            if (process.HasExited)
            {
                throw new InvalidOperationException(
                    $"Dsw2025Tpi.Api finalizó inesperadamente (código {process.ExitCode}).\n{string.Join(Environment.NewLine, logs)}");
            }

            try
            {
                using var response = client.GetAsync($"{baseUrl}/health-check").GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                    return;
            }
            catch
            {
                // Todavía está iniciando
            }

            Thread.Sleep(300);
        }

        throw new TimeoutException(
            $"Dsw2025Tpi.Api no respondió en {baseUrl}/health-check dentro de 45s.\n{string.Join(Environment.NewLine, logs)}");
    }

    private static int GetFreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static string FindApiProjectPath()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "Dsw2025Tpi.Api", "Dsw2025Tpi.Api.csproj");
            if (File.Exists(candidate))
                return candidate;
        }

        throw new FileNotFoundException("No se encontró Dsw2025Tpi.Api/Dsw2025Tpi.Api.csproj para levantar los tests E2E.");
    }

    private static void Stop()
    {
        try
        {
            if (_apiProcess is { HasExited: false })
                _apiProcess.Kill(entireProcessTree: true);
        }
        catch { }
        finally
        {
            _apiProcess?.Dispose();
            _apiProcess = null;
            _baseUrl = null;
        }
    }
}
