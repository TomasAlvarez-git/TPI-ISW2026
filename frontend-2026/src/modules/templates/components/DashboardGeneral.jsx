import { useState } from 'react';
import { NavLink, Outlet, useNavigate, useLocation } from 'react-router-dom';
import useAuth from '../../auth/hook/useAuth';
import Button from '../../shared/components/Button';
import ResponsiveText from '../../shared/components/ResponsiveText';
import Modal from '../../shared/components/Modal';
import LoginForm from '../../auth/components/LoginForm';
import RegisterForm from '../../auth/components/RegisterForm';
import UserButton from '../../auth/components/UserButton';
import SearchBar from '../../shared/components/SearchBar';
import LogoButton from '../../shared/components/LogoButton';

function DashboardGeneral() {
  const [openMenu, setOpenMenu] = useState(false);

  const location = useLocation();

  const navigate = useNavigate();

  const [isOpenLogin, setIsOpenLogin] = useState(false);
  const [isOpenSignup, setIsOpenSignup] = useState(false);
  const isCustomer = localStorage.getItem("customerId") != 'null' ? true : false;

  const openLoginModal = () => {
    setIsOpenLogin(true);
  };

  const openSignupModal = () => {
    setIsOpenSignup(true);
  }
  const getLinkStyles = ({ isActive }) => (
    `
      pl-5 block pb-4 rounded-4xl transition hover:bg-gray-100
          sm:pt-6
          md:pt-6
      ${isActive
      ? 'bg-purple-200 hover:bg-purple-100 '
      : ''
    }
    `
  );

  const renderAuthButtons = (isMobile = false) => {
    const token = localStorage.getItem('token');
    const isLogged = token && token !== 'null';

    if (!isLogged) {
      return (
        <div className={isMobile ? 'flex flex-col gap-4' : 'hidden lg:flex gap-4'}>
          <Button
            onClick={() => openLoginModal()}
          >
            Iniciar Sesión
          </Button>
          <Button
            variant='secondary'
            onClick={() => openSignupModal()}
          >
            Registrarse
          </Button>
        </div>
      );
    } else {
      return (
        <div className={isMobile ? 'flex flex-col gap-4' : 'hidden lg:flex gap-4'}>
          <UserButton className={`${isMobile ? 'block w-full lg:hidden' : 'hidden md:block'}`} />
        </div>
      );
    }
  };

  const renderDesktopNavbar = () => (
    <nav className='lg:block hidden gap-4 px-3 rounded-lg bg-white'>
      <div className='flex justify-between gap-5'>
        <Button
          onClick={() => navigate('/')}
          variant={location.pathname === '/'
            ? 'default'
            : 'secondary'
          }
        >
          Productos
        </Button>
        <Button
          onClick={() => navigate('/cart')}
          variant={location.pathname === '/cart'
            ? 'default'
            : 'secondary'
          }
        >
          Carrito de compras
        </Button>
      </div>
    </nav>
  );
  const [searchTerm, setSearchTerm] = useState(
    new URLSearchParams(location.search).get("search") || ""
  );

  const handleSearch = () => {
    const params = new URLSearchParams(location.search);
    params.set("search", searchTerm);
    navigate(`?${params.toString()}`);
  };
  const renderSearchBar = () => (
    <div className="flex items-center ">
      <SearchBar
        searchTerm={searchTerm}
        setSearchTerm={setSearchTerm}
        handleSearch={handleSearch}
      />
    </div>
  );

  const renderMobileNavLinks = () => (
    <ul className='flex flex-col justify-between'>
      <li>
        <NavLink to='/' className={getLinkStyles} onClick={() => setOpenMenu(false)}>
          <ResponsiveText>Productos</ResponsiveText>
        </NavLink>
      </li>
      <li>
        <NavLink to='/cart' className={getLinkStyles} onClick={() => setOpenMenu(false)}>
          <ResponsiveText>Carrito de compras</ResponsiveText>
        </NavLink>
      </li>
    </ul>
  );

  return (
    <div
      className="
        h-full
        grid
        grid-cols-1
        grid-rows-[auto_1fr]

        sm:gap-3
        sm:grid-cols-1
      "
    >
      <header
        className="
          flex
          items-center
          justify-between
          p-4
          shadow
          rounded
          bg-white
          sm:col-span-1
          sm:px-6 
          sm:py-4
          gap-4
        "
      >
        {/* <img 
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
        /> */}
        <LogoButton />
        {renderDesktopNavbar()}

        <div className='flex-grow sm:flex-grow-0'>
          {renderSearchBar()}
        </div>

        {renderAuthButtons(false)}

        <button
          className="
            bg-transparent
            border-none
            shadow-none
            lg:hidden 
            text-2xl 
            p-1 
            w-10 
            h-10
          "
          onClick={() => setOpenMenu(!openMenu)}
        >
          {openMenu ? <span>&#215;</span> : <span>&#9776;</span>}
        </button>
      </header>

      <aside
        className={`
          absolute
          top-0
          bottom-0
          bg-white
          w-64
          p-6
          ${openMenu ? 'left-0' : 'left-[-256px]'}
          rounded
          shadow
          flex
          z-40
          flex-col
          justify-between
          transition-all duration-300
          lg:hidden 
        `}
      >
        <nav>
          {renderMobileNavLinks()}
          <hr className='opacity-20 mt-4 mb-5' />
          {renderAuthButtons(true)}
        </nav>

      </aside>
      <Modal isOpen={isOpenLogin} onClose={() => setIsOpenLogin(false)}>
        <LoginForm />
      </Modal>
      <Modal isOpen={isOpenSignup} onClose={() => setIsOpenSignup(false)}>
        <RegisterForm hasRole={!isCustomer} />
      </Modal>

      <main
        className="
          p-5
          overflow-y-scroll
        "
      >
        <Outlet />
      </main>
    </div>
  );
};

export default DashboardGeneral;