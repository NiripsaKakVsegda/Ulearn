using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Ulearn.Common.Extensions;

namespace Stepik.Api.Tests
{
	[TestFixture]
	class ResponseParsing
	{
		[Test]
		public void ParseJsonViaJObject()
		{
			var response =
				"{\"stepics\": [{\"profile\": 1279728, \"server_time\": 1503388527.402963, \"config\": {\"can_change_language\": true, \"has_language_selector\": true, \"has_social_sharing\": true, \"login_link\": \"/login/\", \"has_mobile_apps_banner\": true, \"has_registration_link\": true, \"is_standard_footer\": true, \"project_domain\": \"stepik.org\", \"can_change_name\": true, \"has_activity_graph\": true, \"has_lessons_in_navbar\": true, \"course_reviews_passed_percent\": 80, \"has_course_review_tab\": true, \"payments_yandex_money_scid\": \"75883\", \"payments_yandex_money_shop_id\": \"74729\", \"email_verify_warn_delay\": 259200, \"has_extra_favicons\": true, \"index_logo\": \"\", \"is_standard_index\": true, \"project_name\": \"Stepik\", \"project_support_email\": \"support@stepik.org\", \"project_help_center_url\": \"http://stepik.help\", \"language\": null, \"is_public_telegram_bot\": true, \"index_text\": \"\", \"enable_recommended_courses_for_users\": [\"135091\", \"43329\", \"15557637\"], \"is_standard_explore\": true, \"footer_logo\": \"\", \"is_public_social_accounts\": true, \"project_main_instance_url\": \"https://stepik.org/\", \"can_change_city\": true, \"courses_default_publicity\": true, \"stripe_api_public_key\": \"pk_live_vTMgNYTvMxpmKA0pN0ZQYoHT\", \"telegram_bot_name\": \"StepicBot\", \"show_course_welcome_popup\": false, \"payments_yandex_money_form_action\": \"https://money.yandex.ru/eshop.xml\", \"favicon\": \"/static/classic/ico/favicon.ico\", \"is_full_profile\": true, \"can_change_email\": true, \"can_set_password\": true, \"has_email_verification_alert\": true, \"topbar_logo\": \"/static/frontend/topbar_logo.svg\"}, \"user\": 1279728, \"total_submissions\": 29595335, \"id\": 1, \"total_active\": 39714, \"total_quizzes\": 93441}], \"users\": [{\"id\": 1279728, \"profile\": 1279728, \"is_private\": false, \"is_active\": true, \"is_guest\": false, \"is_organization\": false, \"short_bio\": \"\", \"details\": \"\", \"first_name\": \"\\u0410\\u043d\\u0434\\u0440\\u0435\\u0439\", \"last_name\": \"\\u0413\\u0435\\u0439\\u043d\", \"full_name\": \"\\u0410\\u043d\\u0434\\u0440\\u0435\\u0439 \\u0413\\u0435\\u0439\\u043d\", \"alias\": null, \"avatar\": \"https://stepik.org/users/1279728/72786ba3cc74f2d81f93a59c4154079752192e84/avatar.svg\", \"cover\": null, \"level\": 1, \"level_title\": \"user\", \"tag_progresses\": [], \"knowledge\": 0, \"knowledge_rank\": null, \"reputation\": 0, \"reputation_rank\": null, \"join_date\": null, \"social_profiles\": [3348], \"solved_steps_count\": 9, \"created_courses_count\": 0, \"created_lessons_count\": 3, \"issued_certificates_count\": 0}], \"profiles\": [{\"id\": 1279728, \"first_name\": \"\\u0410\\u043d\\u0434\\u0440\\u0435\\u0439\", \"last_name\": \"\\u0413\\u0435\\u0439\\u043d\", \"full_name\": \"\\u0410\\u043d\\u0434\\u0440\\u0435\\u0439 \\u0413\\u0435\\u0439\\u043d\", \"is_private\": false, \"avatar\": \"https://stepik.org/users/1279728/72786ba3cc74f2d81f93a59c4154079752192e84/avatar.svg\", \"language\": \"ru\", \"city\": null, \"short_bio\": \"\", \"details\": \"\", \"subscribed_for_mail\": false, \"notification_email_delay\": \"5m\", \"notification_status\": 1279728, \"is_staff\": false, \"is_guest\": false, \"can_add_lesson\": true, \"can_add_course\": true, \"can_add_group\": false, \"subscribed_for_news_en\": true, \"subscribed_for_news_ru\": false, \"bit_field\": 1, \"level\": 1, \"level_title\": \"user\", \"level_abilities\": [], \"has_password\": false, \"social_accounts\": [27204, 138533, 138534, 138535], \"email_addresses\": [58948], \"is_email_verified\": true, \"invite_url\": \"https://stepik.org/friends/aRpJEY3n\", \"telegram_bot_url\": \"https://telegram.me/StepicBot?start=TVRJM09UY3lPQToxZGs0MjM6NUY1eDdfSktkUWRWRzI3VlpteHBoQ3dqNm1i\", \"balance\": \"0\", \"subscription_plan\": null, \"experiment_choices\": {\"feedback_type\": \"feedback_type.standart\"}, \"allowed_private_courses_count\": 0}]}";

			var jObject = JObject.Parse(response);
			Assert.True(jObject.TryGetValue("stepics", out JToken x));
			Assert.IsNotNull(jObject["stepics"][0]);
			Assert.IsNotEmpty(jObject["stepics"][0].ToString());
			var parsedResponse = jObject["stepics"][0].ToString().DeserializeJson<StepikApiStepic>();
			Assert.AreEqual(parsedResponse.ProfileId, 1279728);
		}
	}
}