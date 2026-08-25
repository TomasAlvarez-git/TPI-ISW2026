import { motion } from "framer-motion";
import StructuredCard from "./StructuredCard";
function AnimatedCard({ keyProp = null, children }) {

    return (
        <motion.div
            key={keyProp}
            initial={{ opacity: 0, y: 30 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: false, amount: 0.3 }}
            transition={{ duration: 0.4, ease: "easeOut" }}
        >
            {children}
        </motion.div>
    );
}
export default AnimatedCard;