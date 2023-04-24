import { useDispatch } from "react-redux";
import { AppDispatch } from "../../../setupStore";

export const useAppDispatch = () => useDispatch<AppDispatch>()
