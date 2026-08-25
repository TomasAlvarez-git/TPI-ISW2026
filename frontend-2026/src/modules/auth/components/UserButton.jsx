import useAuth from '../../auth/hook/useAuth';
import { useState } from 'react';
import ResponsiveText from '../../shared/components/ResponsiveText';
import Button from '../../shared/components/Button';
import { useNavigate } from 'react-router-dom';

function UserButton({ className = '' }){
    const customerIdValue = localStorage.getItem('customerId');
    const userName = localStorage.getItem('userName');
    const isNotCustomer = customerIdValue === 'null' || customerIdValue === null;
    
    const { signOut } = useAuth();
    const [showUserMenu, setShowUserMenu] = useState(false);
    const navigate = useNavigate();

    const logout = () => {
        signOut();
        navigate('/');
    };
    
    const buttonClass = `
        h-10 border border-purple-200 shadow-sm transition duration-150 
        ${className.includes('w-full') ? 'w-full' : 'w-auto'} 
        rounded-full bg-purple-100 flex items-center justify-between px-2 cursor-pointer
        hover:bg-purple-200
    `;

    return(
        <div 
            className={`relative ${className}`}
            onBlur={() => setTimeout(() => setShowUserMenu(false), 200)}
        >
            <button
                className={buttonClass}
                onClick={() => setShowUserMenu((s) => !s)}
                aria-expanded={showUserMenu}
            >
                <div className='flex items-center pl-1'>
                    <ResponsiveText className='text-sm font-medium text-zinc-600 truncate max-w-[100px]'>
                        {userName || 'Usuario'}
                    </ResponsiveText>
                </div>
                
                <div className="h-8 w-8 rounded-full bg-white flex items-center justify-center p-1 border border-purple-300 ml-2">
                    <svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" className='h-5 w-5'>
                        <g id="SVGRepo_iconCarrier"> 
                            <path d="M5 21C5 17.134 8.13401 14 12 14C15.866 14 19 17.134 19 21M16 7C16 9.20914 14.2091 11 12 11C9.79086 11 8 9.20914 8 7C8 4.79086 9.79086 3 12 3C14.2091 3 16 4.79086 16 7Z" stroke="#62516c" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"></path> 
                        </g>
                    </svg>
                </div>
            </button>

            {showUserMenu && (
                <div className="absolute right-0 mt-2 w-35 bg-white shadow-xl rounded-lg p-2 z-50 border border-gray-100 origin-top-right animate-fade-in">
                    {isNotCustomer && (
                        <Button 
                            variant='secondary'
                            onClick={() => { setShowUserMenu(false); navigate('admin/home'); }}
                        >
                            Menu admin
                        </Button>
                    )}
                    
                    {isNotCustomer && <hr className='my-1 opacity-10' />}

                    <Button 
                        variant='secondary' 
                        onClick={() => { setShowUserMenu(false); logout();}}
                    >
                        Cerrar sesion
                    </Button>
                </div>
            )}
        </div>
    );
}
export default UserButton;