import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Button from '../../shared/components/Button';
import { getGeneralProducts } from '../services/list';
import Pagination from '../../shared/components/Pagination';
import StructuredCard from '../../shared/components/StructuredCard';
import Counter from '../../shared/components/Counter';
import ResponsiveText from '../../shared/components/ResponsiveText';
import Modal from '../../shared/components/Modal';
import AnimatedCard from '../../shared/components/AnimatedCard';

function ListGeneralProductsPage() {
    const navigate = useNavigate();

    const [pageNumber, setPageNumber] = useState(1);
    const [pageSize, setPageSize] = useState(10);

    const [total, setTotal] = useState(0);
    const [products, setProducts] = useState([]);
    const [loading, setLoading] = useState(false);

    const [modalInfo, setModalInfo] = useState({
        open: false,
        productName: '',
    });

    const [selectedQuantities, setSelectedQuantities] = useState({});
    const [errors, setErrors] = useState({});

    const queryParams = new URLSearchParams(location.search);
    const urlSearchTerm = queryParams.get('search') || '';

    const handleCountChange = (sku, count) => {

        const product = products.find(p => p.sku === sku);

        setSelectedQuantities(prev => ({
            ...prev,
            [sku]: count,
        }));

        if (product && count > Number(product.stockQuantity)) {
            setErrors(prev => ({
                ...prev,
                [sku]: `No puedes seleccionar más de ${product.stockQuantity} unidades.`
            }));
        } else if (product && count === Number(product.stockQuantity)) {
            setErrors(prev => ({
                ...prev,
                [sku]: `Este es el último producto en stock.`
            }));
        } else {
            setErrors(prev => ({
                ...prev,
                [sku]: null
            }));
        }
    };

    const handleAddToCart = (product) => {
        const currentCount = Number(selectedQuantities[product.sku] ?? 0);
        if (currentCount < 1) {
            setErrors(prev => ({
                ...prev,
                [product.sku]: 'Debes agregar al menos 1 producto.'
            }));
            return;
        }

        setErrors(prev => ({
            ...prev,
            [product.sku]: null
        }));

        let cart = [];
        try {
            const storedCart = localStorage.getItem('cart');
            if (storedCart) {
                cart = JSON.parse(storedCart);
            }
        } catch (error) {
            console.error('Error al leer el carrito de localStorage:', error);
        }
        const newCartItem = {
            productId: product.id,
            sku: product.sku,
            name: product.name,
            unitPrice: product.currentUnitPrice,
            quantity: currentCount,
            stockQuantity: product.stockQuantity
        };
        const existingIndex = cart.findIndex(item => item.sku === product.sku);

        if (existingIndex > -1) {
            cart[existingIndex].quantity = currentCount;
            cart[existingIndex].stockQuantity = product.stockQuantity;
        } else {
            cart.push(newCartItem);
        }
        try {
            localStorage.setItem('cart', JSON.stringify(cart));

            setModalInfo({
                open: true,
                productName: product.name
            });

        } catch (error) {
            console.error('Error al guardar el carrito en localStorage:', error);
        }
    };

    const fetchProducts = async () => {
        try {
            setLoading(true);
            const { data, error } = await getGeneralProducts(urlSearchTerm, pageNumber, pageSize);

            if (error) throw error;

            setTotal(data.totalFiltered ?? 0);
            setProducts(data.products ?? []);
        } catch (error) {
            console.error(error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        const storedCart = JSON.parse(localStorage.getItem('cart')) || [];

        const quantitiesMap = {};
        storedCart.forEach(item => {
            quantitiesMap[item.sku] = item.quantity;
        });

        setSelectedQuantities(quantitiesMap);
    }, []);

    useEffect(() => {
        fetchProducts();
    }, [pageSize, pageNumber, urlSearchTerm]);

    const totalPages = Math.ceil(total / pageSize);
    const noProducts = products.length === 0;

    const renderProducts = () => {
        if (loading) {
            return <ResponsiveText>Buscando datos...</ResponsiveText>;
        }

        if (noProducts) {
            return (
                <div className='p-4 text-center'>
                    <ResponsiveText as='p' className='font-semibold'>
                        No se encontraron productos.
                    </ResponsiveText>
                </div>
            );
        }
        return products.map((product) => (
            <AnimatedCard key={product.sku}>
                <StructuredCard
                    key={product.sku}
                    className='p-4'
                    title={(
                        <div className='w-full h-48 rounded-xl flex items-center justify-center overflow-hidden bg-gray-100 mb-3'>
                            <svg width="74px" height="74px" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" stroke="#62516c" strokeWidth="0.2">
                                <path d="M3.28034 2.21968C2.98745 1.92678 2.51257 1.92677 2.21968 2.21966C1.92678 2.51255 1.92677 2.98743 2.21966 3.28032L3.48327 4.54395C3.17684 5.03983 3 5.62427 3 6.25001V17.75C3 19.5449 4.45507 21 6.25 21H17.75C18.3757 21 18.96 20.8232 19.4559 20.5169L20.7194 21.7805C21.0123 22.0734 21.4872 22.0734 21.7801 21.7805C22.073 21.4876 22.073 21.0127 21.7801 20.7198L3.28034 2.21968ZM12.0681 13.1289L18.2739 19.3349C18.2929 19.3572 18.3118 19.3794 18.3305 19.4014C18.1489 19.4653 17.9535 19.5 17.75 19.5H6.25C6.04613 19.5 5.8504 19.4651 5.66845 19.4011L11.4752 13.7148L12.0681 13.1289ZM11.1189 12.1797C10.8678 12.2865 10.6322 12.4409 10.4258 12.643L4.60326 18.3437C4.53643 18.1583 4.5 17.9584 4.5 17.75V6.25001C4.5 6.04371 4.5357 5.84575 4.60125 5.66196L11.1189 12.1797Z" fill="#62516c"></path> <path d="M19.5 16.3183V6.25001C19.5 5.28351 18.7165 4.50001 17.75 4.50001H7.68194L6.18265 3.00069C6.20505 3.00024 6.2275 3.00001 6.25 3.00001H17.75C19.5449 3.00001 21 4.45508 21 6.25001V17.75C21 17.7726 20.9998 17.7952 20.9993 17.8176L19.5 16.3183Z" fill="#62516c"></path> <path d="M15.2521 6.50001C16.4959 6.50001 17.5042 7.50832 17.5042 8.75212C17.5042 9.99593 16.4959 11.0042 15.2521 11.0042C14.0083 11.0042 13 9.99593 13 8.75212C13 7.50832 14.0083 6.50001 15.2521 6.50001ZM15.2521 8.00001C14.8367 8.00001 14.5 8.33674 14.5 8.75212C14.5 9.16751 14.8367 9.50424 15.2521 9.50424C15.6675 9.50424 16.0042 9.16751 16.0042 8.75212C16.0042 8.33674 15.6675 8.00001 15.2521 8.00001Z" fill="#62516c"></path> </svg>
                        </div>
                    )}
                    content={
                        <div className='flex flex-col w-full'>
                            <ResponsiveText as='h2' className='font-semibold text-xl'>
                                {product.name}
                            </ResponsiveText>

                            <ResponsiveText as='p' className='text-sm mt-1 mb-3'>
                                ${product.currentUnitPrice}
                            </ResponsiveText>

                            <div className='flex items-center justify-between mt-2'>
                                <Counter
                                    stock={product.stockQuantity}
                                    initialCount={selectedQuantities[product.sku] || 0}
                                    onCountChange={(count) => handleCountChange(product.sku, count)}
                                />

                                <Button
                                    onClick={() => handleAddToCart(product)}
                                >
                                    Agregar
                                </Button>
                            </div>
                            {errors[product.sku] && (
                                <p className='text-red-600 text-sm mt-2 w-full text-center'>
                                    {errors[product.sku]}
                                </p>
                            )}
                        </div>
                    }
                />
            </AnimatedCard>

        ))
    };

    return (
        <div>
            <div className='grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4 p-4'>
                {renderProducts()}
            </div>

            <Pagination
                pageNumber={pageNumber}
                totalPages={totalPages}
                pageSize={pageSize}
                setPageNumber={setPageNumber}
                setPageSize={setPageSize}
            />

            <Modal
                isOpen={modalInfo.open}
                onClose={() => setModalInfo({ open: false, productName: "" })}
            >
                <h2 className='text-lg font-semibold mb-4'>Agregaste a tu carrito:</h2>
                <div className='flex justify-center mb-5'>
                    <ResponsiveText as='p' className='font-bold'>
                        {modalInfo.productName}
                    </ResponsiveText>
                </div>


                <div className='flex justify-between gap-3'>
                    <Button
                        variant='secondary'
                        onClick={() => setModalInfo({ open: false, productName: '' })}
                    >
                        Ver más productos
                    </Button>

                    <Button
                        onClick={() => {
                            setModalInfo({ open: false, productName: '' });
                            navigate('/cart');
                        }}
                    >
                        Ir al carrito
                    </Button>
                </div>
            </Modal>
        </div>
    );
}

export default ListGeneralProductsPage;