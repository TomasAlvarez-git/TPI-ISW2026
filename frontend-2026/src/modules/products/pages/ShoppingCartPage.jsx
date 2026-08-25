import Button from "../../shared/components/Button";
import StructuredCard from "../../shared/components/StructuredCard";
import Counter from "../../shared/components/Counter";
import ResponsiveText from "../../shared/components/ResponsiveText";
import Modal from "../../shared/components/Modal";
import Input from "../../shared/components/Input";
import LoginForm from "../../auth/components/LoginForm";
import { useEffect, useState, useCallback, useMemo, useRef } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import OrderManager from "../../orders/components/OrderManager";
import AnimatedCard from "../../shared/components/AnimatedCard";

function ShoppingCartPage() {
    const navigate = useNavigate();
    const location = useLocation();
    const orderManagerRef = useRef(null);
    const [errors, setErrors] = useState({});
    const [cartItems, setCartItems] = useState([]);
    const [isOpenLogin, setIsOpenLogin] = useState(false);

    const queryParams = new URLSearchParams(location.search);
    const urlSearchTerm = queryParams.get('search') || '';
    const [isSubmitting, setIsSubmitting] = useState(false);

    const loadCartFromLocalStorage = useCallback(() => {
        try {
            const storedCart = localStorage.getItem("cart");
            if (storedCart) {
                setCartItems(JSON.parse(storedCart));
            } else {
                setCartItems([]);
            }
        } catch (error) {
            console.error("Error al cargar el carrito:", error);
            setCartItems([]);
        }
    }, []);

    useEffect(() => {
        loadCartFromLocalStorage();
    }, [loadCartFromLocalStorage]);

    const updateCartItemQuantity = useCallback((sku, newQuantity) => {
        setCartItems((prev) => {
            let updatedCart;

            if (newQuantity < 1) {
                updatedCart = prev.filter((item) => item.sku !== sku);
            } else {
                updatedCart = prev.map((item) =>
                    item.sku === sku ? { ...item, quantity: newQuantity } : item
                );
            }

            try {
                localStorage.setItem("cart", JSON.stringify(updatedCart));
            } catch (e) {
                console.error("Error al guardar carrito:", e);
            }

            return updatedCart;
        });
    }, []);

    const handleCounterChange = useCallback(
        (sku, count) => {
            updateCartItemQuantity(sku, count);
        },
        [updateCartItemQuantity]
    );

    const handleRemoveItem = useCallback(
        (sku) => {
            updateCartItemQuantity(sku, 0);
        },
        [updateCartItemQuantity]
    );

    const filteredCartItems = useMemo(() => {
        const term = urlSearchTerm.toLowerCase().trim();
        if (!term) {
            return cartItems;
        }

        return cartItems.filter(item =>
            item.name.toLowerCase().includes(term) ||
            item.sku.toLowerCase().includes(term)
        );
    }, [cartItems, urlSearchTerm]);

    const { totalQuantity, totalPrice } = useMemo(() => {
        let totalQty = 0;
        let totalPrc = 0;

        cartItems.forEach((item) => {
            const quantity = Number(item.quantity) || 0;
            const price = Number(item.unitPrice) || 0;

            totalQty += quantity;
            totalPrc += quantity * price;
        });

        return {
            totalQuantity: totalQty,
            totalPrice: totalPrc.toFixed(2),
        };
    }, [cartItems]);

    const openLoginModal = () => {
        setIsOpenLogin(true);
    };

    const triggerOrder = () => {
        if (orderManagerRef.current) {
            orderManagerRef.current.triggerOrder();
        }
    };

    const delay = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

    const manageOrder = async () => {
        const token = localStorage.getItem("token");
        const customerId = localStorage.getItem("customerId");
        const cartItems = JSON.parse(localStorage.getItem("cart")) || [];
        const orderItems = cartItems.map(item => ({
            productId: item.productId,
            quantity: Number(item.quantity)
        }));

        if (!token || token.length < 10) {
            setErrors(prev => ({ ...prev, customerId: "Debes iniciar sesión para continuar" }));
            await delay(1500);
            setErrors(prev => ({ ...prev, customerId: "" }));
            openLoginModal();
            return;
        }

        if (customerId == "null") {
            setErrors(prev => ({ ...prev, customerId: "No es un cliente válido, por favor inicie sesion como cliente" }));
            await delay(2000);
            setErrors(prev => ({ ...prev, customerId: "" }));
            openLoginModal();
            return;
        }

        if (orderItems.length === 0) {
            setErrors(prev => ({
                ...prev,
                orderItems: "El carrito está vacío o contiene productos no válidos."
            }));
            return;
        }

        setErrors(prev => ({
            ...prev,
            customerId: null,
            orderItems: null
        }));

        triggerOrder();
    };
    const isCartEmpty = filteredCartItems.length === 0;

    return (
        <>
            {isCartEmpty ? (
                <div className="p-4 text-center">
                    <ResponsiveText as="p" className="font-semibold">
                        Tu carrito está vacío.
                    </ResponsiveText>

                    <Button className="mt-4" onClick={() => navigate("/")}>
                        Ir a Comprar
                    </Button>
                </div>
            ) : (
                <div className="w-full max-w-7xl mx-auto grid grid-cols-1 lg:grid-cols-3 gap-4 pb-28 lg:pb-0">
                    <div className="lg:col-span-2 flex flex-col gap-4 pb-40">
                        {filteredCartItems.map((item) => (
                            <AnimatedCard key={item.sku}>
                                <StructuredCard
                                    key={item.sku}
                                    className={""}
                                    title={item.name}
                                    content={
                                        <div className="flex flex-col text-2xl">
                                            <ResponsiveText as="p" className="text-gray-600 text-xl gap-1">
                                                Cantidad de productos: {item.quantity}
                                            </ResponsiveText>

                                            <ResponsiveText as="p" className="text-gray-600 text-xl gap-1">
                                                Sub Total: ${((item.quantity || 0) * (item.unitPrice || 0)).toFixed(2)}
                                            </ResponsiveText>

                                            <div className="flex items-center justify-between gap-1 mt-4">
                                                <Counter
                                                    initialCount={item.quantity}
                                                    stock={item.stockQuantity}
                                                    activeReset={false}
                                                    onCountChange={(count) => handleCounterChange(item.sku, count)}
                                                />
                                                <Button

                                                    onClick={() => handleRemoveItem(item.sku)}
                                                >
                                                    Borrar
                                                </Button>
                                            </div>
                                        </div>
                                    }
                                />
                            </AnimatedCard>
                        ))}
                    </div>

                    <StructuredCard
                        className="flex flex-col z-20 
                                    fixed bottom-0 left-0 right-0 
                                    w-full lg:h-[calc(100vh-160px)] 
                                    lg:top-4 lg:sticky"
                        title="Detalle del pedido"
                        content={
                            <>
                                <ResponsiveText className="text-gray-600 text-xl sm:mt-1">Cantidad total: {totalQuantity}</ResponsiveText>
                                <ResponsiveText className="text-gray-600 text-xl sm:mt-1">Precio total: ${totalPrice}</ResponsiveText>
                            </>
                        }
                        actions={
                            <>
                                {errors.customerId && (
                                    <p className="text-red-600 text-sm mt-2">{errors.customerId}</p>
                                )}

                                {errors.orderItems && (
                                    <p className="text-red-600 text-sm mt-2">{errors.orderItems}</p>
                                )}

                                <Button className="w-full mt-2 font-semibold" onClick={manageOrder}>
                                    Finalizar compra
                                </Button>
                            </>
                        }
                        contentClassName={'flex flex-col'}
                    />
                </div>
            )}

            <Modal isOpen={isOpenLogin} onClose={() => setIsOpenLogin(false)}>
                <LoginForm modal={true} redirectTo={location.pathname} />
            </Modal>

            <OrderManager
                ref={orderManagerRef}
                cartItems={cartItems}
                onSuccess={() => {
                    setCartItems([]);
                    navigate("/");
                }}
            />
        </>
    );

}

export default ShoppingCartPage;
