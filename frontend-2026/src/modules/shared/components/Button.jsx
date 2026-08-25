function Button({ children, type = 'button', variant = 'default', ...restProps }) {
  if (!['button', 'reset', 'submit'].includes(type)) {
    console.warn('type prop not supported');
  }

  const variantStyle = {
    default: 'bg-purple-200 hover:bg-purple-300 transition px-2 py-1 rounded-lg text-[clamp(15px,2.5vw,18px)]',
    secondary: 'bg-gray-100 hover:bg-gray-200 transition px-2 py-1 rounded-lg text-[clamp(15px,2.5vw,18px)]',
  };

  const extraClass = restProps.className || '';

  return (
    <button
      {...restProps}
      className={`${variantStyle[variant]} ${extraClass}`.trim()}
      type={type}
    >
      {children}
    </button>
  );
};

export default Button;
