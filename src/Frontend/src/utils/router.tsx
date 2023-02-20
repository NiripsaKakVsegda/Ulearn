import React, { ComponentType, FunctionComponent, useEffect } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import {
	MatchParams,
	WithNavigate,
	WithLocation,
	WithParams,
	WithRouter,
} from "../models/router";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "src/redux/reducers";
import { changeCurrentCourseAction } from "src/actions/course";

export function withOldRouter<T extends WithRouter>(Component: React.ComponentType<T>)
	: FunctionComponent<Omit<T, keyof WithRouter>> {
	return (props: Omit<T, keyof WithRouter>) => {
		const location = useLocation();
		const navigate = useNavigate();
		const params = useParams<keyof MatchParams>();
		return (
			<Component
				{ ...props as T }
				location={ location }
				params={ params }
				navigate={ navigate }
			/>
		);
	};
}

export function withLocation<T extends WithLocation>(Component: React.ComponentType<T>)
	: FunctionComponent<Omit<T, keyof WithLocation>> {
	return (props: Omit<T, keyof WithLocation>) => {
		const location = useLocation();
		return (
			<Component
				{ ...props as T }
				location={ location }
			/>
		);
	};
}

export function withNavigate<T extends WithNavigate>(Component: React.ComponentType<T>)
	: FunctionComponent<Omit<T, keyof WithNavigate>> {
	return (props: Omit<T, keyof WithNavigate>) => {
		const navigate = useNavigate();
		return (
			<Component
				{ ...props as T }
				navigate={ navigate }
			/>
		);
	};
}

export function withParams<T extends WithParams>(Component: React.ComponentType<T>)
	: FunctionComponent<Omit<T, keyof WithParams>> {
	return (props: Omit<T, keyof WithParams>) => {
		const params = useParams<keyof MatchParams>();
		return (
			<Component
				{ ...props as T }
				params={ params }
			/>
		);
	};
}

export function withCourseRouting<T extends Partial<WithParams>>(Child: ComponentType<T>)
	: FunctionComponent<Omit<T, keyof WithParams>> {
	return (props) => {
		const dispatch = useDispatch();
		const params = useParams<keyof MatchParams>();
		const currentCourseId = useSelector<RootState>(state => state.courses.currentCourseId);

		useEffect(() => {
			if(params.courseId && params.courseId !== currentCourseId) {
				dispatch(changeCurrentCourseAction(params.courseId));
			}
		}, [currentCourseId]);

		return (<Child { ...props as T } params={ params }/>);
	};
}
