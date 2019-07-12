import React, { Component } from "react";
import PropTypes from "prop-types";
import styles from "./CourseNavigationItem.less";
import classnames from "classnames";
import ProgressBar from "../../ProgressBar";
import { courseMenuItemType } from "../../types"


class CourseNavigationItem extends Component {
	render () {
		const { title, isActive } = this.props;

		const classes = {
			[styles.itemLink]: true,
			[styles.active]: isActive
		};

		return (
			<li className={ styles.root } onClick={ this.clickHandle }>
				<div className={ classnames(classes) }>
					<span className={ styles.text }>{ title }</span>
					{ this.renderProgress() }
				</div>
			</li>
		);
	}

	renderProgress() {
		const { progress } = this.props;

		if (progress) {
			return (
				<span className={ styles.progressWrapper }>
					<ProgressBar value={ progress } small color={ progress >= 1 ? 'green' : 'blue' } />
				</span>
			);
		}
	}

	clickHandle = () => {
		this.props.onClick(this.props.id);
	};
}

CourseNavigationItem.propTypes = courseMenuItemType;

export default CourseNavigationItem

