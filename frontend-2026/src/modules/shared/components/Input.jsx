import ResponsiveText from "./ResponsiveText";
function Input({ label, error = '', ...restProps }) {
  return (
    <div
      className='
        flex
        flex-col
      '
    >
      <ResponsiveText>{label}:</ResponsiveText>
      <ResponsiveText as='input' className={error && 'border-red-400'} {...restProps} />
      {error && <ResponsiveText className="text-red-500 text-base sm:text-xs">{error}</ResponsiveText>}
    </div>
  );
};

export default Input;
