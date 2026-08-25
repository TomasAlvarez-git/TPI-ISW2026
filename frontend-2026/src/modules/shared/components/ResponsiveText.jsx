function ResponsiveText({ as: Component = "span", children, className = "", ...rest }) {
    const isTextTag = ["span", "p", "h1", "h2", "h3", "h4", "h5", "h6", "input", "ol"].includes(Component);

    return (
        <Component
            className={`${isTextTag && !className.includes("text-") ? "text-[clamp(16px,2vw,18px)]" : ""} ${className}`}

            {...rest}
        >
            {children}
        </Component>
    );
}

export default ResponsiveText;
