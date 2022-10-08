import 'webpack-jquery-ui';
import 'webpack-jquery-ui/draggable';
import 'webpack-jquery-ui/sortable';
import 'webpack-jquery-ui/droppable';
import 'webpack-jquery-ui/css';
import 'webpack-jquery-ui/autocomplete';
import 'webpack-jquery-ui/menu';
import 'webpack-jquery-ui/tabs';
import 'webpack-jquery-ui/tooltip';
import 'webpack-jquery-ui/selectable';
import 'webpack-jquery-ui/selectmenu';
import 'bootstrap';

import 'bootstrap/dist/css/bootstrap.min.css';
import 'src/legacy/styles/font-awesome.global.css';
import 'src/legacy/styles/ulearn.global.css';

//libs overrides
import bootsrapSelectOverride from './scripts/bootstrap-select-override.js'; // переписываем select, который используется, например, на странице ведомости курса

//legacy scripts
import tooltips from './scripts/tooltips.js'; //тултипы, пробелмы с аккаунтом, замечания бота и тд
import buttons from './scripts/buttons.js'; //кнопка загрузки курса
import slideChecking from './scripts/slide-checking.js'; //сабмиты задач
import slideEditor from './scripts/slide-editor.js'; //code mirror editor
import slideModals from './scripts/slide-modals.js'; //модалки
import spoiler from './scripts/spoiler.js'; //спойлеры
import slideQuiz from './scripts/slide-quiz.js'; // слайд с квизами
import slideSpoilers from './scripts/slide-spoilers.js'; //спойлеры, TODO reuse react
import slideComments from './scripts/slide-comments.js'; //обработчики для комментариев, который рисует бэк на странице /Admin/Comments
import slideTex from './scripts/slide-tex.js'; //tex, TODO reuse react
import slideUtils from './scripts/slide-utils.js'; // панельки статистик, авто апдейтеры
import slideVersions from './scripts/slide-versions.js'; //версии сабмитов к задачкам
import certificatesEditor from './scripts/certificates-editor.js'; //сертификаты
import analyticsHighcharts from './scripts/analytics-highcharts.js'; // графики статистик
import analytics from './scripts/analytics.js'; // другие баллы к ревью задачек, открытие следующей задачки на ревью
import courseStatistics from './scripts/course-statistics.js'; //ведомость курса
import suggestMailNotifications from './scripts/suggest-mail-notifications.js'; // модалка для получения новостей о курсе на email
import diagnostics from './scripts/diagnostics.js'; // измененные слайды при загрузке курса
import formsReplaceAction from './scripts/forms.replace-action.js'; // изменения в формах
import notificationsSettings from './scripts/notifications-settings.js'; // настройки уведомлений в профиле
import profile from './scripts/profile.js'; // профиль
import notifications from './scripts/notifications.js'; // уведомления, переключение между табами и тд
import stepik from './scripts/stepik.js'; // степик?
import modals from './scripts/modals.js'; // модалки, автофокусы
import connectCheckboxes from './scripts/connect-checkboxes.js'; // используется в очереди на ручную проверку
import { antiplagiarism, antiplagiarismChart } from './scripts/antiplagiarism.js'; // антиплагиат
import styleErrorsSettings from './scripts/style-errors-settings.js'; // стилевые ошибки c#
import studentSubmissions from './scripts/student-submissions.js'; // поиск решений студентов
import reactRenderer from './scripts/react-renderer.js'; // рендер реакта для бэка
import smoothScroll from './scripts/smoothScroll.js'; //скролл к фильтрам + показ/скрытие

//scripts used in cshtml by backend renderer
import loginForContinue from './scripts/slide-guest.js'; //модалка для логина не авторизованных пользователей, TODO reuse react modal
import {
	openPopup,
	ToggleButtonClass,
	ToggleDropDownClass,
	ToggleSystemRoleOrAccess,
} from './scripts/users-list.js';
import { submitQuiz } from './scripts/slide-quiz.js';
import { ShowPanel } from './scripts/slide-utils.js';

/*deprecated section
name -> status, description, reason for status
respond -> deleted, полифил для matchMedia, не поддерживаем старые браузеры
likely -> deleted, кнопки поделиться с друзьями в сертификатах, переехал в шаблон сертификата
slide-hints -> deleted, подсказки к задачкам, на реакте
slide-popovers -> deleted, подсказка к ускорение видео, на реакте
slide-run -> deleted, запуск задачи, на реакте
slide-diff -> deleted, сравнение вывода к задачкам, на реакте
activateClipboard -> deleted, копирование текста в буфер обмена для ссылки в сертификатах, переехал в шаблон сертификата
grader -> deleted, клиенты грейдера, не используется
groupsEditor -> deleted, страница с группами, переехало на реакт
 */

const documentReadyFunctions = [
	//overrides
	bootsrapSelectOverride,
	//scripts
	tooltips,
	buttons,
	slideChecking,
	slideEditor,
	slideModals,
	slideQuiz,
	slideSpoilers,
	spoiler,
	slideTex,
	slideComments,
	slideUtils,
	slideVersions,
	certificatesEditor,
	analyticsHighcharts,
	analytics,
	courseStatistics,
	suggestMailNotifications,
	diagnostics,
	formsReplaceAction,
	notificationsSettings,
	profile,
	notifications,
	stepik,
	modals,
	connectCheckboxes,
	antiplagiarism,
	antiplagiarismChart,
	styleErrorsSettings,
	studentSubmissions,
	smoothScroll,
	reactRenderer,
];

window.legacy = {
	...window.legacy,
	documentReadyFunctions,
	loginForContinue,
	ToggleSystemRoleOrAccess,
	ToggleButtonClass,
	ToggleDropDownClass,
	openPopup,
	submitQuiz,
	ShowPanel,
};

export default documentReadyFunctions;
