export function toggleRoleOrCourseAccess(comment) {
	const target = window.legacy.toggleRoles.target;
	const toggleClass = window.legacy.toggleRoles.toggleClass;
	const $object = $(target);
	const url = $object.data("toggleUrl");
	const token = $('#AntiForgeryTokenContainer input[name="__RequestVerificationToken"]').val();
	$.ajax({
		url: url,
		method: "POST",
		data: {
			__RequestVerificationToken: token,
			isEnabled: !$object.data('hasAccess'),
			comment: comment
		},
		dataType: 'json',
	})
		.done(function (result) {
			if(result.status !== 'ok') {
				alert(result.message);
				return;
			}
			$object.data('hasAccess', !$object.data('hasAccess'));
			toggleClass($object);
		});
}

export function ToggleSystemRoleOrAccess(target, toggleClass) {
	const $object = $(target);
	const url = $object.data("toggleUrl");
	const token = $('#AntiForgeryTokenContainer input[name="__RequestVerificationToken"]').val();
	$.ajax({
		url: url,
		method: "POST",
		data: {
			__RequestVerificationToken: token,
			isEnabled: !$object.data('hasAccess'),
		},
		dataType: 'json',
	})
		.done(function (result) {
			if(result.status !== 'ok') {
				console.error(result.message);
				return;
			}
			$object.data('hasAccess', !$object.data('hasAccess'));
			toggleClass($object);
		});
}

export function ToggleButtonClass(button) {
	button.toggleClass(button.data("css-class"));
}

export function ToggleDropDownClass(dropdownElement) {
	const parent = $(dropdownElement.parents()[0]);
	const cssClass = parent.data("css-class");
	dropdownElement.toggleClass(cssClass);
	const elements = dropdownElement.siblings().addBack();
	const button = $(parent.siblings("button")[0]);
	const buttonCss = button.data("css-class");
	button.removeClass(buttonCss);
	for (let i = 0; i < elements.length; ++i) {
		const elem = $(elements[i]);
		if(elem.hasClass(cssClass)) {
			button.addClass(buttonCss);
			break;
		}
	}
}

export function openPopup(target, toggleClass) {
	if(($(target)).data("css-class") === "btn-danger") {
		ToggleSystemRoleOrAccess(target, toggleClass);
		return;
	}

	const $object = $(target);
	const userName = $object.data("toggleUsername");
	const roleOrAccess = $object.data("toggleRole");
	const isRole = $object.data("toggleIsrole") === 'True';
	const courseTitle = $object.data("toggleCoursetitle");
	let isGrant = true;
	const className = ((target)).className;
	if(className.indexOf("btn-success") > -1 || className.indexOf("btn-warning") > -1 || className.indexOf("btn-info") > -1) {
		isGrant = false;
	}

	if(className === "li-info" || className === "li-warning" || className === "li-success") {
		isGrant = false;
	}

	window.legacy.toggleRoles = {
		target: target,
		toggleClass: toggleClass,
		userName: userName,
		role: roleOrAccess,
		isRole: isRole,
		courseTitle: courseTitle,
		isGrant: isGrant
	};

	const root = document.querySelector('.react-rendered');
	root.setAttribute('modalOpened', true);
}

export function closePopup() {
	const root = document.querySelector('.react-rendered');
	root.setAttribute('modalOpened', false);
}
