
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getOrders } from '../services/list';
import StructuredCard from '../../shared/components/StructuredCard';
import Pagination from '../../shared/components/Pagination';
import SearchBar from '../../shared/components/SearchBar';
import ResponsiveText from '../../shared/components/ResponsiveText';
import ListCard from '../../shared/components/ListCard'

const orderStatus = {
    ALL: 'all',
    PENDING: 'pending',
    PROCESSING: 'processing',
    SHIPPED: 'shipped',
    DELIVERED: 'delivered',
};

function ListOrdersPage() {
    const navigate = useNavigate();

    const [searchTerm, setSearchTerm] = useState('');
    const [status, setStatus] = useState(orderStatus.ALL);
    const [pageNumber, setPageNumber] = useState(1);
    const [pageSize, setPageSize] = useState(10);

    const [total, setTotal] = useState(0);
    const [orders, setOrders] = useState([]);

    const [loading, setLoading] = useState(false);
    const [openOrderId, setOpenOrderId] = useState(null);

    const handleClicked = (id) => {
        setOpenOrderId(prev => prev === id ? null : id);
    };

    const fetchOrders = async () => {
        try {
            setLoading(true);
            const statusToSend = status === orderStatus.ALL ? null : status;
            const { dataOrders, errorOrders } = await getOrders(searchTerm, statusToSend, pageNumber, pageSize);

            if (errorOrders) throw errorOrders;

            setTotal(dataOrders.totalFiltered ?? 0);

            setOrders(Array.isArray(dataOrders.orderItems)
                ? dataOrders.orderItems
                : (Array.isArray(dataOrders.orders)
                    ? dataOrders.orders
                    : (Array.isArray(dataOrders.Orders) ? dataOrders.Orders : [])
                )
            );
        } catch (err) {
            console.error('Error fetchOrders:', err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchOrders();
    }, [status, pageSize, pageNumber, searchTerm]);

    const totalPages = Math.ceil(total / pageSize);

    const handleSearch = () => {
        setPageNumber(1);
        fetchOrders();
    };

    const noOrders = orders.length === 0;

    const renderOrderList = () => {
        if (loading) {
            return <ResponsiveText>Buscando datos...</ResponsiveText>;
        }

        if (noOrders) {
            return (
                <div className='p-4 text-center'>
                    <ResponsiveText as='p' className='font-semibold'>
                        No cuentas con órdenes en tu base de datos.
                    </ResponsiveText>
                </div>
            );
        }

        return orders.map(order => (
            <ListCard
                key={order.id}
                keyProp={order.id}
                openId={openOrderId}
                handleClicked={handleClicked}
                title={`Orden #${order.id} - ${order.customerName}`}
                closedContent={`Estado: ${order.status} | Total: $${order.totalAmount}`}
                openedContent={(
                    <>
                        <li>Fecha: {order.date.slice(0, 10)}</li>
                        <li>Direccion de entrega: {order.shippingAddress}</li>
                        <li>Direccion de facturacion: {order.billingAddress}</li>
                        <li>Notas: {order.notes}</li>
                        <li>Items:</li>
                        {order.orderItems?.map((item) => (
                            <li key={item.productId} className='ml-4'>
                                - {item.name} | {item.quantity}
                            </li>
                        ))}
                    </>
                )}
            />
        ));
    };

    return (
        <div>
            <StructuredCard
                className='mb-4 p-4'
                title='Ordenes'
                actions={
                    <>
                        <div
                            className='flex items-center gap-3'
                        >
                            <SearchBar
                                searchTerm={searchTerm}
                                setSearchTerm={setSearchTerm}
                                handleSearch={handleSearch}
                            ></SearchBar>
                        </div>
                        <select onChange={evt => {
                            setStatus(evt.target.value);
                            setPageNumber(1);
                        }} className='text-[1.3rem]'>
                            <option value={orderStatus.ALL}>Todas</option>
                            <option value={orderStatus.PENDING}>Pendientes</option>
                            <option value={orderStatus.PROCESSING}>En Proceso</option>
                            <option value={orderStatus.SHIPPED}>Enviadas</option>
                            <option value={orderStatus.DELIVERED}>Entregadas</option>
                        </select>
                    </>
                }
                actionsClassName={'flex flex-row justify-between flex-wrap'}
            ></StructuredCard>

            <div className='mt-4 flex flex-col gap-4'>
                {renderOrderList()}
            </div>

            <Pagination
                pageNumber={pageNumber}
                totalPages={totalPages}
                pageSize={pageSize}
                setPageNumber={setPageNumber}
                setPageSize={setPageSize}
            />
        </div>
    );
};

export default ListOrdersPage;