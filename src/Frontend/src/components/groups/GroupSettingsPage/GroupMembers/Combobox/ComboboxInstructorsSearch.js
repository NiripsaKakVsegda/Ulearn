import React, { Component } from "react";
import PropTypes from "prop-types";
import api from "src/api";
import { ComboBox } from "ui";
import Avatar from "src/components/common/Avatar/Avatar";

import styles from './comboboxInstructorsSearch.less';

class ComboboxInstructorsSearch extends Component {
	state = {query: ''};

	render() {
		const {selected} = this.props;
		return (
			<ComboBox
				getItems={this.getItems}
				size="small"
				width="100%"
				value={selected}
				renderItem={this.renderItem}
				renderValue={this.renderItem}
				renderNotFound={this.renderNotFound}
				onValueChange={this.onChangeItem}
				onInputChange={query => this.setState({query})}
				placeholder="Начните вводить имя, фамилию или логин преподавателя" />
		);
	}

	getItems = (query) => {
		const {accesses, owner} = this.props;
		const isNotAddedUser = (item) => {
			return (owner.id !== item.id) && accesses.every(i => i.user.id !== item.id);
		};

		return api.users.getCourseInstructors(this.props.courseId, query)
			.then(json => {
				return json.users
					.map(item => item.user)
					.filter(isNotAddedUser)
					.map(item => ({
								value: item.id,
								label: item.visibleName,
								...item,
							}
						)
					)
			})
			.catch(error => {
				console.error(error);
				return [];
			});
	};

	renderItem = (item) => {
		const name = item.label;

		return (
			<div className={styles["teacher"]}>
				<Avatar user={item} size='small' />
				<span>{name}</span>
				<span className={styles["teacher-login"]}>логин: {item.login}</span>
			</div>
		)
	};

	renderNotFound = () => {
		const msg = 'Не найдено. Возможно, вы не выдали права на странице «Студенты и преподаватели»';

		return <span>{msg}</span>;
	};

	onChangeItem = (item) => {
		this.props.onAddTeacher(item);
	}
}

ComboboxInstructorsSearch.propTypes = {
	selected: PropTypes.object,
	courseId: PropTypes.string,
	onAddTeacher: PropTypes.func,
	accesses: PropTypes.array,
	owner: PropTypes.object,
};

export default ComboboxInstructorsSearch;
