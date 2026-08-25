import ResponsiveText from './ResponsiveText';
import SelectStatus from './SelectStatus';

function Pagination({ pageNumber, totalPages, pageSize, setPageNumber, setPageSize
}) {
    return (
        <div className="flex justify-center items-center mt-3 gap-3">
            <button
                disabled={pageNumber === 1}
                onClick={() => setPageNumber(pageNumber - 1)}
                className="bg-gray-200 px-3 py-1 rounded-md disabled:bg-gray-100 h-8 flex items-center justify-center">
                <ResponsiveText>Atras</ResponsiveText>
            </button>

            <ResponsiveText
                as="span"
                className="px-3 py-1 h-8 flex items-center justify-center">
                {pageNumber} / {totalPages}
            </ResponsiveText>

            <button
                disabled={pageNumber === totalPages}
                onClick={() => setPageNumber(pageNumber + 1)}
                className="bg-gray-200 px-3 py-1 rounded-md disabled:bg-gray-100 h-8 flex items-center justify-center">
                <ResponsiveText>Siguiente</ResponsiveText>
            </button>
            <SelectStatus
                value={pageSize}
                onChange={(value) => {
                    setPageNumber(1);
                    setPageSize(Number(value));
                }}
                options={[
                    { value: 10, label: "10" },
                    { value: 2, label: "2" },
                    { value: 15, label: "15" },
                    { value: 20, label: "20" }
                ]}
            />
        </div>
    );
}
export default Pagination;