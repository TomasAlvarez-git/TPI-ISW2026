function Card({ children, className }) {
  return (
    <div className={`bg-white shadow-sm border border-gray-200 rounded-2xl p-4 ${className}`}>
      {children}
    </div>
  );
}

export default Card;
