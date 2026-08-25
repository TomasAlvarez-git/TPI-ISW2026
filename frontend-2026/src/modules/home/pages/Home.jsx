import { useEffect, useState } from 'react';
import { getProducts } from '../../products/services/list';
import { getOrders } from '../../orders/services/list';
import StructuredCard from '../../shared/components/StructuredCard';
import AnimatedCard from '../../shared/components/AnimatedCard';

function Home() {
    const [productsCount, setProductsCount] = useState(null);
    const [ordersCount, setOrdersCount] = useState(0);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        async function loadCounts() {
            setLoading(true);

            const { data, error } = await getProducts();
            if (!error) {
                setProductsCount(data.totalCount ?? 0);
            } else {
                console.error('Error fetching product count', error);
                setProductsCount(null);
            }

            const { dataOrders, errorOrders } = await getOrders();
            if (!errorOrders) {
                setOrdersCount(dataOrders.totalCount ?? 0);
            } else {
                console.error('Error fetching order count', errorOrders);
                setOrdersCount(null);
            }

            setLoading(false);
        }

        loadCounts();
    }, []);

    return (
        <div className='flex flex-col gap-3 sm:grid sm:grid-cols-2'>
            <AnimatedCard >
                <StructuredCard
                title='Productos'
                content={
                    <p className="text-sm text-gray-500 mt-1">
                        {loading ? 'Cargando...' :`Cantidad de productos: ${productsCount}`}
                    </p>}
                >
                </StructuredCard>
            </AnimatedCard>
            <AnimatedCard>
                <StructuredCard
                    title='Órdenes'
                    content={
                        <p className='text-sm text-gray-500 mt-1'>
                            {loading ? 'Cargando...' : `Cantidad de ordenes: ${ordersCount}`}
                        </p>
                    }>
                </StructuredCard>
            </AnimatedCard>
        </div>
    );
}

export default Home;
