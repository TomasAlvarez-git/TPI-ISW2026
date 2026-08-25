import React from "react";
import ResponsiveText from "./ResponsiveText";

function SelectStatus({ value, onChange, options }) {
    return (
        <select
            value={value}
            onChange={evt => onChange(evt.target.value)}
            className="text-[1.3rem] p-2 rounded border"
        >
            {options.map(opt => (
                <option
                    key={opt.value}
                    value={opt.value}
                    className="text-[clamp(16px,2vw,20px)] leading-normal"
                >
                    {opt.label}
                </option>
            ))}
        </select>
    );
}
export default SelectStatus;