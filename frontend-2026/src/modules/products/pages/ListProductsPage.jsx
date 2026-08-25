import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Button from '../../shared/components/Button';
import { getProducts } from '../services/list';
import ResponsiveText from '../../shared/components/ResponsiveText';
import StructuredCard from '../../shared/components/StructuredCard';
import Pagination from '../../shared/components/Pagination';
import SelectStatus from '../../shared/components/SelectStatus';
import SearchBar from '../../shared/components/SearchBar';
import ListCard from '../../shared/components/ListCard';

const productStatus = {
  ALL: 'all',
  ENABLED: 'active',
  DISABLED: 'inactive',
};

function ListProductsPage() {
  const navigate = useNavigate();

  const [searchTerm, setSearchTerm] = useState('');
  const [status, setStatus] = useState(productStatus.ALL);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const [total, setTotal] = useState(0);
  const [products, setProducts] = useState([]);

  const [loading, setLoading] = useState(false);
  const [openProductId, setOpenProductId] = useState(null);

  const fetchProducts = async () => {
    try {
      setLoading(true);
      const { data, error } = await getProducts(searchTerm, status, pageNumber, pageSize);

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
    fetchProducts();
  }, [status, pageSize, pageNumber]);

  const totalPages = Math.ceil(total / pageSize);

  const handleSearch = async () => {
    await fetchProducts();
  };

  const handleClicked = (sku) => {
    setOpenProductId(prev => prev === sku ? null : sku);
  };

  const noProducts = products.length === 0;

  const renderProductsList = () => {
    if (loading) {
      return <ResponsiveText>Buscando datos...</ResponsiveText>;
    }

    if (noProducts) {
      return (
        <div className='p-4 text-center'>
          <ResponsiveText as='p' className='font-semibold'>
            No cuentas con productos en tu base de datos.
          </ResponsiveText>
        </div>
      );
    }
    return products.map(product => (
      <ListCard
        key={product.sku}
        keyProp={product.sku}
        openId={openProductId}
        handleClicked={handleClicked}
        title={`${product.sku} - ${product.name}`}
        closedContent={`Stock: ${product.stockQuantity} - ${product.isActive ? 'Activado' : 'Desactivado'}`}
        openedContent={
          <>
            <li>{`Precio: ${product.currentUnitPrice}`}</li>
            <li>{`Descripción: ${product.description ? product.description : '-'}`}</li>
          </>

        }
      />
    ));
  }
  return (
    <div>
      <StructuredCard
        className='mb-4 p-4'
        content={
          <>
            <ResponsiveText as="h2" className={'text-2xl font-semibold'}>Productos</ResponsiveText>
            <Button
              className='h-13 w-13 rounded-2xl sm:hidden'
              onClick={() => navigate('/admin/products/create')}
            >
              <svg viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg"><g id="SVGRepo_bgCarrier" strokeWidth="0"></g><g id="SVGRepo_tracerCarrier" strokeLinecap="round" strokeLinejoin="round"></g><g id="SVGRepo_iconCarrier"> <path d="M5 11C4.44772 11 4 10.5523 4 10C4 9.44772 4.44772 9 5 9H15C15.5523 9 16 9.44772 16 10C16 10.5523 15.5523 11 15 11H5Z" fill="#000000"></path> <path d="M9 5C9 4.44772 9.44772 4 10 4C10.5523 4 11 4.44772 11 5V15C11 15.5523 10.5523 16 10 16C9.44772 16 9 15.5523 9 15V5Z" fill="#000000"></path> </g></svg>
            </Button>

            <Button
              className='hidden sm:block'
              onClick={() => navigate('/admin/products/create')}
            >
              Crear Producto
            </Button>
          </>
        }
        actions={
          <>
            <div className='flex items-center gap-3'>
              <SearchBar
                searchTerm={searchTerm}
                setSearchTerm={setSearchTerm}
                handleSearch={handleSearch}
              ></SearchBar>
            </div>
            <SelectStatus
              value={status}
              onChange={setStatus}
              options={[
                { value: productStatus.ALL, label: 'Todos' },
                { value: productStatus.ENABLED, label: 'Habilitados' },
                { value: productStatus.DISABLED, label: 'Inhabilitados' }
              ]}
            />
          </>
        }
        actionsClassName={'flex flex-row justify-between flex-wrap'}
        contentClassName={'flex flex-row justify-between'}
      ></StructuredCard>

      <div className='mt-4 flex flex-col gap-4'>
        {renderProductsList()}
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

export default ListProductsPage;
