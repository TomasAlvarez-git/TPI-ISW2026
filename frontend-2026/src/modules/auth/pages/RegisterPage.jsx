import RegisterForm from '../components/RegisterForm';

function RegisterPage() {
    const isCustomer = localStorage.getItem("customerId") != 'null' ? true : false;
    return (
    <div className='
        flex
        flex-col
        justify-center
        h-[100dvh]
        bg-neutral-100
        items-center
    '>
        <RegisterForm hasRole={!isCustomer} />
    </div>
    );
}

export default RegisterPage;