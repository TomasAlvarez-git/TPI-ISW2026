import { useNavigate } from "react-router-dom";
function LogoButton() {
    const navigate = useNavigate();
    return (
        <>
            <button className='shadow-none' onClick={() => navigate('/')}>
                <img
                    src="/logoCompletoEcommerce.png"
                    alt="Logo"
                    width="200"
                    className='hidden sm:block'
                />
                <img
                    src="/logoEcommerce1.png"
                    alt="Small Logo"
                    width="40"
                    className='sm:hidden'
                />
            </button>

        </>
    );

}
export default LogoButton;