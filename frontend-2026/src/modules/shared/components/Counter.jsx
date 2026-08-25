import Button from "./Button";
import Input from "./Input";
import { useState, useEffect } from "react";

function Counter(props) {
  const [count, setCount] = useState(props.initialCount || 0);
  const [hasError, setHasError] = useState(false);

  useEffect(() => {
    if (props.onCountChange) {
      props.onCountChange(count);
    }
  }, [count]);

  useEffect(() => {
    if (typeof props.initialCount === 'number' && props.initialCount !== count) {
      setCount(props.initialCount);
    }
  }, [props.initialCount]);

  const increase = () => {
    /*
    Number(props.stock);
    if (count < props.stock) {
      setCount(count + 1);
      setHasError(false);
    } else {
      setHasError(true);
    }*/
    const maxStock = Number(props.stock);
    if (!isNaN(maxStock) && count >= maxStock) {
      setHasError(true);
      return;
    }
    setCount(count + 1);
    setHasError(false);
  };

  const decrease = () => {
    if (count > 0) {
      setCount(count - 1);
      setHasError(false);
    }
  };

  const reset = () => {
    setCount(0);
    setHasError(false);
  }

  const handleInputChange = (event) => {
    const value = parseInt(event.target.value, 10);
    const maxStock = Number(props.stock);
    
    if (isNaN(value) || value < 0) {
      setCount(0);
      setHasError(false);
    }else if (!isNaN(maxStock) && value > maxStock) {
      setCount(maxStock);
      setHasError(true);
    }
    else {
      setCount(value);
      setHasError(false);
    }
  };

  const handleKeyDown = (event) => {
    const maxStock = Number(props.stock);
    if (event.key === "ArrowUp" && !isNaN(maxStock) && count >= maxStock) {
      event.preventDefault();
    }
  };

  return (
    <div>
      <div className="flex items-center space-x-2">
        <button
          className='h-7 w-7 rounded-2xl'
          onClick={decrease}
          disabled={count <= 0}
        >
          <svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <g id="SVGRepo_bgCarrier" strokeWidth="0"></g><g id="SVGRepo_tracerCarrier" strokeLinecap="round" strokeLinejoin="round"></g>
            <g id="SVGRepo_iconCarrier"> <path d="M6 12L18 12" stroke="#000000" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"></path> </g>
          </svg>
        </button>

        <input
          type="number"
          value={count}
          onChange={handleInputChange}
          onKeyDown={handleKeyDown}
          min="0"
          max={props.stock}
          className={`border text-center text-sm h-7 w-12 rounded-lg ${hasError ? 'border-red-500' : 'border-gray-200'}`}
        >
        </input>

        <button
          className='h-7 w-7 rounded-2xl'
          onClick={increase}
          disabled={!isNaN(Number(props.stock)) && count >= Number(props.stock)}
        >
          <svg viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
            <g id="SVGRepo_bgCarrier" strokeWidth="0"></g><g id="SVGRepo_tracerCarrier" strokeLinecap="round" strokeLinejoin="round"></g>
            <g id="SVGRepo_iconCarrier"> <path d="M5 11C4.44772 11 4 10.5523 4 10C4 9.44772 4.44772 9 5 9H15C15.5523 9 16 9.44772 16 10C16 10.5523 15.5523 11 15 11H5Z" fill="#000000"></path> <path d="M9 5C9 4.44772 9.44772 4 10 4C10.5523 4 11 4.44772 11 5V15C11 15.5523 10.5523 16 10 16C9.44772 16 9 15.5523 9 15V5Z" fill="#000000"></path> </g>
          </svg>
        </button>
        {props.activeReset && <Button className="ml-4" onClick={reset}>Borrar</Button>}
        {hasError && (
          <p className="text-red-500 text-sm mt-2 w-full text-center">
            Máximo de stock alcanzado
          </p>
        )}

        {/* NO ESTA ANDANDO ESTO DE LOS ERRORES */}
      </div>

    </div>

  );
};

export default Counter;