CREATE OR REPLACE FUNCTION print_notice(msg text) 
  RETURNS integer AS 
$$ 
DECLARE 
BEGIN 
    RAISE NOTICE USING MESSAGE = msg;
    RETURN null;
END; 
$$ 
LANGUAGE 'plpgsql' IMMUTABLE; 

SELECT print_notice('after data copy');

ALTER TABLE public."AspNetUsers"
    DROP COLUMN "Names";

ALTER TABLE public."AspNetUsers"
    ADD "Names" text GENERATED ALWAYS AS (public.immutable_concat_ws(' '::text, VARIADIC ARRAY[NULLIF(("UserName")::text, ''::text), NULLIF("FirstName", ''::text), NULLIF("LastName", ''::text), NULLIF("FirstName", ''::text)])) STORED;

ALTER TABLE public."Likes"
    RENAME COLUMN "ID" TO "Id";

DROP TABLE IF EXISTS public."__MigrationHistory";

SELECT print_notice('update sequences');

SELECT setval('antiplagiarism."Clients_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM antiplagiarism."Clients"), 1), false);

SELECT setval('antiplagiarism."Codes_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM antiplagiarism."Codes"), 1), false);

SELECT setval('antiplagiarism."OldSubmissionsInfluenceBorder_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM antiplagiarism."OldSubmissionsInfluenceBorder"), 1), false);

SELECT setval('antiplagiarism."SnippetsOccurences_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM antiplagiarism."SnippetsOccurences"), 1), false);

SELECT setval('antiplagiarism."SnippetsStatistics_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM antiplagiarism."SnippetsStatistics"), 1), false);

SELECT setval('antiplagiarism."Snippets_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM antiplagiarism."Snippets"), 1), false);

SELECT setval('antiplagiarism."Submissions_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM antiplagiarism."Submissions"), 1), false);

SELECT setval('antiplagiarism."TaskStatisticsSourceData_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM antiplagiarism."TaskStatisticsSourceData"), 1), false);

SELECT setval('antiplagiarism."WorkQueueItems_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM antiplagiarism."WorkQueueItems"), 1), false);

SELECT setval('public."AdditionalScores_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."AdditionalScores"), 1), false);

SELECT setval('public."AspNetRoleClaims_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."AspNetRoleClaims"), 1), false);

SELECT setval('public."AspNetUserClaims_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."AspNetUserClaims"), 1), false);

SELECT setval('public."AutomaticExerciseCheckings_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."AutomaticExerciseCheckings"), 1), false);

SELECT setval('public."CommentLikes_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."CommentLikes"), 1), false);

SELECT setval('public."Comments_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."Comments"), 1), false);

SELECT setval('public."CourseAccesses_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."CourseAccesses"), 1), false);

SELECT setval('public."CourseFiles_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."CourseFiles"), 1), false);

SELECT setval('public."CourseGitRepos_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."CourseGitRepos"), 1), false);

SELECT setval('public."EnabledAdditionalScoringGroups_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."EnabledAdditionalScoringGroups"), 1), false);

SELECT setval('public."ExerciseCodeReviewComments_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."ExerciseCodeReviewComments"), 1), false);

SELECT setval('public."ExerciseCodeReviews_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."ExerciseCodeReviews"), 1), false);

SELECT setval('public."ExerciseSolutionByGraders_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."ExerciseSolutionByGraders"), 1), false);

SELECT setval('public."FeedViewTimestamps_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."FeedViewTimestamps"), 1), false);

SELECT setval('public."GroupAccesses_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."GroupAccesses"), 1), false);

SELECT setval('public."GroupLabels_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."GroupLabels"), 1), false);

SELECT setval('public."GroupMembers_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."GroupMembers"), 1), false);

SELECT setval('public."Groups_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."Groups"), 1), false);

SELECT setval('public."LabelOnGroups_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."LabelOnGroups"), 1), false);

SELECT setval('public."LastVisits_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."LastVisits"), 1), false);

SELECT setval('public."Likes_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."Likes"), 1), false);

SELECT setval('public."LtiConsumers_ConsumerId_seq"', COALESCE((SELECT MAX("ConsumerId")+1 FROM public."LtiConsumers"), 1), false);

SELECT setval('public."LtiSlideRequests_RequestId_seq"', COALESCE((SELECT MAX("RequestId")+1 FROM public."LtiSlideRequests"), 1), false);

SELECT setval('public."ManualExerciseCheckings_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."ManualExerciseCheckings"), 1), false);

SELECT setval('public."NotificationDeliveries_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."NotificationDeliveries"), 1), false);

SELECT setval('public."NotificationTransportSettings_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."NotificationTransportSettings"), 1), false);

SELECT setval('public."NotificationTransports_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."NotificationTransports"), 1), false);

SELECT setval('public."Notifications_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."Notifications"), 1), false);

SELECT setval('public."SlideHints_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."SlideHints"), 1), false);

SELECT setval('public."SlideRates_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."SlideRates"), 1), false);

SELECT setval('public."StepikAccessTokens_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."StepikAccessTokens"), 1), false);

SELECT setval('public."StepikExportProcesses_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."StepikExportProcesses"), 1), false);

SELECT setval('public."StepikExportSlideAndStepMaps_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."StepikExportSlideAndStepMaps"), 1), false);

SELECT setval('public."SystemAccesses_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."SystemAccesses"), 1), false);

SELECT setval('public."UnitAppearances_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."UnitAppearances"), 1), false);

SELECT setval('public."UserExerciseSubmissions_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."UserExerciseSubmissions"), 1), false);

SELECT setval('public."UserFlashcardsUnlocking_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."UserFlashcardsUnlocking"), 1), false);

SELECT setval('public."UserFlashcardsVisits_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."UserFlashcardsVisits"), 1), false);

SELECT setval('public."UserQuestions_QuestionId_seq"', COALESCE((SELECT MAX("QuestionId")+1 FROM public."UserQuestions"), 1), false);

SELECT setval('public."UserQuizAnswers_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."UserQuizAnswers"), 1), false);

SELECT setval('public."UserQuizSubmissions_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."UserQuizSubmissions"), 1), false);

SELECT setval('public."UserRoles_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."UserRoles"), 1), false);

SELECT setval('public."Visits_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."Visits"), 1), false);

SELECT setval('public."WorkQueueItems_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."WorkQueueItems"), 1), false);

SELECT setval('public."XQueueExerciseSubmissions_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."XQueueExerciseSubmissions"), 1), false);

SELECT setval('public."XQueueWatchers_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."XQueueWatchers"), 1), false);

SELECT print_notice('create foreign keys');


--
-- TOC entry 3731 (class 2606 OID 57165)
-- Name: MostSimilarSubmissions FK_MostSimilarSubmissions_Submissions_SimilarSubmissionId; Type: FK CONSTRAINT; Schema: antiplagiarism; Owner: ulearn
--

ALTER TABLE ONLY antiplagiarism."MostSimilarSubmissions"
    ADD CONSTRAINT "FK_MostSimilarSubmissions_Submissions_SimilarSubmissionId" FOREIGN KEY ("SimilarSubmissionId") REFERENCES antiplagiarism."Submissions"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3732 (class 2606 OID 57170)
-- Name: MostSimilarSubmissions FK_MostSimilarSubmissions_Submissions_SubmissionId; Type: FK CONSTRAINT; Schema: antiplagiarism; Owner: ulearn
--

ALTER TABLE ONLY antiplagiarism."MostSimilarSubmissions"
    ADD CONSTRAINT "FK_MostSimilarSubmissions_Submissions_SubmissionId" FOREIGN KEY ("SubmissionId") REFERENCES antiplagiarism."Submissions"("Id") ON DELETE CASCADE;


--
-- TOC entry 3733 (class 2606 OID 57182)
-- Name: SnippetsOccurences FK_SnippetsOccurences_Snippets_SnippetId; Type: FK CONSTRAINT; Schema: antiplagiarism; Owner: ulearn
--

ALTER TABLE ONLY antiplagiarism."SnippetsOccurences"
    ADD CONSTRAINT "FK_SnippetsOccurences_Snippets_SnippetId" FOREIGN KEY ("SnippetId") REFERENCES antiplagiarism."Snippets"("Id") ON DELETE CASCADE;


--
-- TOC entry 3734 (class 2606 OID 57187)
-- Name: SnippetsOccurences FK_SnippetsOccurences_Submissions_SubmissionId; Type: FK CONSTRAINT; Schema: antiplagiarism; Owner: ulearn
--

ALTER TABLE ONLY antiplagiarism."SnippetsOccurences"
    ADD CONSTRAINT "FK_SnippetsOccurences_Submissions_SubmissionId" FOREIGN KEY ("SubmissionId") REFERENCES antiplagiarism."Submissions"("Id") ON DELETE CASCADE;


--
-- TOC entry 3729 (class 2606 OID 57150)
-- Name: SnippetsStatistics FK_SnippetsStatistics_Clients_ClientId; Type: FK CONSTRAINT; Schema: antiplagiarism; Owner: ulearn
--

ALTER TABLE ONLY antiplagiarism."SnippetsStatistics"
    ADD CONSTRAINT "FK_SnippetsStatistics_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES antiplagiarism."Clients"("Id") ON DELETE CASCADE;


--
-- TOC entry 3730 (class 2606 OID 57155)
-- Name: SnippetsStatistics FK_SnippetsStatistics_Snippets_SnippetId; Type: FK CONSTRAINT; Schema: antiplagiarism; Owner: ulearn
--

ALTER TABLE ONLY antiplagiarism."SnippetsStatistics"
    ADD CONSTRAINT "FK_SnippetsStatistics_Snippets_SnippetId" FOREIGN KEY ("SnippetId") REFERENCES antiplagiarism."Snippets"("Id") ON DELETE CASCADE;


--
-- TOC entry 3727 (class 2606 OID 57133)
-- Name: Submissions FK_Submissions_Clients_ClientId; Type: FK CONSTRAINT; Schema: antiplagiarism; Owner: ulearn
--

ALTER TABLE ONLY antiplagiarism."Submissions"
    ADD CONSTRAINT "FK_Submissions_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES antiplagiarism."Clients"("Id") ON DELETE CASCADE;


--
-- TOC entry 3728 (class 2606 OID 57138)
-- Name: Submissions FK_Submissions_Codes_ProgramId; Type: FK CONSTRAINT; Schema: antiplagiarism; Owner: ulearn
--

ALTER TABLE ONLY antiplagiarism."Submissions"
    ADD CONSTRAINT "FK_Submissions_Codes_ProgramId" FOREIGN KEY ("ProgramId") REFERENCES antiplagiarism."Codes"("Id") ON DELETE CASCADE;


--
-- TOC entry 3735 (class 2606 OID 57199)
-- Name: TaskStatisticsSourceData FK_TaskStatisticsSourceData_Submissions_Submission1Id; Type: FK CONSTRAINT; Schema: antiplagiarism; Owner: ulearn
--

ALTER TABLE ONLY antiplagiarism."TaskStatisticsSourceData"
    ADD CONSTRAINT "FK_TaskStatisticsSourceData_Submissions_Submission1Id" FOREIGN KEY ("Submission1Id") REFERENCES antiplagiarism."Submissions"("Id") ON DELETE CASCADE;


--
-- TOC entry 3736 (class 2606 OID 57204)
-- Name: TaskStatisticsSourceData FK_TaskStatisticsSourceData_Submissions_Submission2Id; Type: FK CONSTRAINT; Schema: antiplagiarism; Owner: ulearn
--

ALTER TABLE ONLY antiplagiarism."TaskStatisticsSourceData"
    ADD CONSTRAINT "FK_TaskStatisticsSourceData_Submissions_Submission2Id" FOREIGN KEY ("Submission2Id") REFERENCES antiplagiarism."Submissions"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3738 (class 2606 OID 57361)
-- Name: AdditionalScores FK_AdditionalScores_AspNetUsers_InstructorId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AdditionalScores"
    ADD CONSTRAINT "FK_AdditionalScores_AspNetUsers_InstructorId" FOREIGN KEY ("InstructorId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3739 (class 2606 OID 57366)
-- Name: AdditionalScores FK_AdditionalScores_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AdditionalScores"
    ADD CONSTRAINT "FK_AdditionalScores_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3737 (class 2606 OID 57349)
-- Name: AspNetRoleClaims FK_AspNetRoleClaims_AspNetRoles_RoleId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AspNetRoleClaims"
    ADD CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."AspNetRoles"("Id") ON DELETE CASCADE;


--
-- TOC entry 3740 (class 2606 OID 57381)
-- Name: AspNetUserClaims FK_AspNetUserClaims_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AspNetUserClaims"
    ADD CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3741 (class 2606 OID 57394)
-- Name: AspNetUserLogins FK_AspNetUserLogins_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AspNetUserLogins"
    ADD CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3742 (class 2606 OID 57407)
-- Name: AspNetUserRoles FK_AspNetUserRoles_AspNetRoles_RoleId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."AspNetRoles"("Id") ON DELETE CASCADE;


--
-- TOC entry 3743 (class 2606 OID 57412)
-- Name: AspNetUserRoles FK_AspNetUserRoles_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3744 (class 2606 OID 57425)
-- Name: AspNetUserTokens FK_AspNetUserTokens_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AspNetUserTokens"
    ADD CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3773 (class 2606 OID 57772)
-- Name: AutomaticExerciseCheckings FK_AutomaticExerciseCheckings_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AutomaticExerciseCheckings"
    ADD CONSTRAINT "FK_AutomaticExerciseCheckings_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3774 (class 2606 OID 57777)
-- Name: AutomaticExerciseCheckings FK_AutomaticExerciseCheckings_TextBlobs_CompilationErrorHash; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AutomaticExerciseCheckings"
    ADD CONSTRAINT "FK_AutomaticExerciseCheckings_TextBlobs_CompilationErrorHash" FOREIGN KEY ("CompilationErrorHash") REFERENCES public."TextBlobs"("Hash") ON DELETE RESTRICT;


--
-- TOC entry 3775 (class 2606 OID 57782)
-- Name: AutomaticExerciseCheckings FK_AutomaticExerciseCheckings_TextBlobs_DebugLogsHash; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AutomaticExerciseCheckings"
    ADD CONSTRAINT "FK_AutomaticExerciseCheckings_TextBlobs_DebugLogsHash" FOREIGN KEY ("DebugLogsHash") REFERENCES public."TextBlobs"("Hash") ON DELETE RESTRICT;


--
-- TOC entry 3776 (class 2606 OID 57787)
-- Name: AutomaticExerciseCheckings FK_AutomaticExerciseCheckings_TextBlobs_OutputHash; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AutomaticExerciseCheckings"
    ADD CONSTRAINT "FK_AutomaticExerciseCheckings_TextBlobs_OutputHash" FOREIGN KEY ("OutputHash") REFERENCES public."TextBlobs"("Hash") ON DELETE RESTRICT;


--
-- TOC entry 3790 (class 2606 OID 57924)
-- Name: AutomaticQuizCheckings FK_AutomaticQuizCheckings_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AutomaticQuizCheckings"
    ADD CONSTRAINT "FK_AutomaticQuizCheckings_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3791 (class 2606 OID 57929)
-- Name: AutomaticQuizCheckings FK_AutomaticQuizCheckings_UserQuizSubmissions_Id; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AutomaticQuizCheckings"
    ADD CONSTRAINT "FK_AutomaticQuizCheckings_UserQuizSubmissions_Id" FOREIGN KEY ("Id") REFERENCES public."UserQuizSubmissions"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3772 (class 2606 OID 57757)
-- Name: CertificateTemplateArchives FK_CertificateTemplateArchives_CertificateTemplates_Certificat~; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."CertificateTemplateArchives"
    ADD CONSTRAINT "FK_CertificateTemplateArchives_CertificateTemplates_Certificat~" FOREIGN KEY ("CertificateTemplateId") REFERENCES public."CertificateTemplates"("Id") ON DELETE CASCADE;


--
-- TOC entry 3769 (class 2606 OID 57734)
-- Name: Certificates FK_Certificates_AspNetUsers_InstructorId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Certificates"
    ADD CONSTRAINT "FK_Certificates_AspNetUsers_InstructorId" FOREIGN KEY ("InstructorId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3770 (class 2606 OID 57739)
-- Name: Certificates FK_Certificates_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Certificates"
    ADD CONSTRAINT "FK_Certificates_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3771 (class 2606 OID 57744)
-- Name: Certificates FK_Certificates_CertificateTemplates_TemplateId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Certificates"
    ADD CONSTRAINT "FK_Certificates_CertificateTemplates_TemplateId" FOREIGN KEY ("TemplateId") REFERENCES public."CertificateTemplates"("Id") ON DELETE CASCADE;


--
-- TOC entry 3777 (class 2606 OID 57799)
-- Name: CommentLikes FK_CommentLikes_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."CommentLikes"
    ADD CONSTRAINT "FK_CommentLikes_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3778 (class 2606 OID 57804)
-- Name: CommentLikes FK_CommentLikes_Comments_CommentId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."CommentLikes"
    ADD CONSTRAINT "FK_CommentLikes_Comments_CommentId" FOREIGN KEY ("CommentId") REFERENCES public."Comments"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3745 (class 2606 OID 57440)
-- Name: Comments FK_Comments_AspNetUsers_AuthorId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Comments"
    ADD CONSTRAINT "FK_Comments_AspNetUsers_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3746 (class 2606 OID 57455)
-- Name: CourseAccesses FK_CourseAccesses_AspNetUsers_GrantedById; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."CourseAccesses"
    ADD CONSTRAINT "FK_CourseAccesses_AspNetUsers_GrantedById" FOREIGN KEY ("GrantedById") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3747 (class 2606 OID 57460)
-- Name: CourseAccesses FK_CourseAccesses_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."CourseAccesses"
    ADD CONSTRAINT "FK_CourseAccesses_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3779 (class 2606 OID 57819)
-- Name: CourseFiles FK_CourseFiles_CourseVersions_CourseVersionId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."CourseFiles"
    ADD CONSTRAINT "FK_CourseFiles_CourseVersions_CourseVersionId" FOREIGN KEY ("CourseVersionId") REFERENCES public."CourseVersions"("Id") ON DELETE CASCADE;


--
-- TOC entry 3748 (class 2606 OID 57473)
-- Name: CourseVersions FK_CourseVersions_AspNetUsers_AuthorId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."CourseVersions"
    ADD CONSTRAINT "FK_CourseVersions_AspNetUsers_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3780 (class 2606 OID 57834)
-- Name: EnabledAdditionalScoringGroups FK_EnabledAdditionalScoringGroups_Groups_GroupId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."EnabledAdditionalScoringGroups"
    ADD CONSTRAINT "FK_EnabledAdditionalScoringGroups_Groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES public."Groups"("Id") ON DELETE CASCADE;


--
-- TOC entry 3811 (class 2606 OID 58105)
-- Name: ExerciseCodeReviewComments FK_ExerciseCodeReviewComments_AspNetUsers_AuthorId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ExerciseCodeReviewComments"
    ADD CONSTRAINT "FK_ExerciseCodeReviewComments_AspNetUsers_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3812 (class 2606 OID 58110)
-- Name: ExerciseCodeReviewComments FK_ExerciseCodeReviewComments_ExerciseCodeReviews_ReviewId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ExerciseCodeReviewComments"
    ADD CONSTRAINT "FK_ExerciseCodeReviewComments_ExerciseCodeReviews_ReviewId" FOREIGN KEY ("ReviewId") REFERENCES public."ExerciseCodeReviews"("Id") ON DELETE CASCADE;


--
-- TOC entry 3808 (class 2606 OID 58080)
-- Name: ExerciseCodeReviews FK_ExerciseCodeReviews_AspNetUsers_AuthorId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ExerciseCodeReviews"
    ADD CONSTRAINT "FK_ExerciseCodeReviews_AspNetUsers_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3809 (class 2606 OID 58085)
-- Name: ExerciseCodeReviews FK_ExerciseCodeReviews_ManualExerciseCheckings_ExerciseCheckin~; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ExerciseCodeReviews"
    ADD CONSTRAINT "FK_ExerciseCodeReviews_ManualExerciseCheckings_ExerciseCheckin~" FOREIGN KEY ("ExerciseCheckingId") REFERENCES public."ManualExerciseCheckings"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3810 (class 2606 OID 58090)
-- Name: ExerciseCodeReviews FK_ExerciseCodeReviews_UserExerciseSubmissions_SubmissionId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ExerciseCodeReviews"
    ADD CONSTRAINT "FK_ExerciseCodeReviews_UserExerciseSubmissions_SubmissionId" FOREIGN KEY ("SubmissionId") REFERENCES public."UserExerciseSubmissions"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3799 (class 2606 OID 58001)
-- Name: ExerciseSolutionByGraders FK_ExerciseSolutionByGraders_GraderClients_ClientId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ExerciseSolutionByGraders"
    ADD CONSTRAINT "FK_ExerciseSolutionByGraders_GraderClients_ClientId" FOREIGN KEY ("ClientId") REFERENCES public."GraderClients"("Id") ON DELETE CASCADE;


--
-- TOC entry 3800 (class 2606 OID 58006)
-- Name: ExerciseSolutionByGraders FK_ExerciseSolutionByGraders_UserExerciseSubmissions_Submissio~; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ExerciseSolutionByGraders"
    ADD CONSTRAINT "FK_ExerciseSolutionByGraders_UserExerciseSubmissions_Submissio~" FOREIGN KEY ("SubmissionId") REFERENCES public."UserExerciseSubmissions"("Id") ON DELETE CASCADE;


--
-- TOC entry 3788 (class 2606 OID 57902)
-- Name: FeedViewTimestamps FK_FeedViewTimestamps_NotificationTransports_TransportId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."FeedViewTimestamps"
    ADD CONSTRAINT "FK_FeedViewTimestamps_NotificationTransports_TransportId" FOREIGN KEY ("TransportId") REFERENCES public."NotificationTransports"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3749 (class 2606 OID 57483)
-- Name: GraderClients FK_GraderClients_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."GraderClients"
    ADD CONSTRAINT "FK_GraderClients_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3781 (class 2606 OID 57846)
-- Name: GroupAccesses FK_GroupAccesses_AspNetUsers_GrantedById; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."GroupAccesses"
    ADD CONSTRAINT "FK_GroupAccesses_AspNetUsers_GrantedById" FOREIGN KEY ("GrantedById") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3782 (class 2606 OID 57851)
-- Name: GroupAccesses FK_GroupAccesses_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."GroupAccesses"
    ADD CONSTRAINT "FK_GroupAccesses_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3783 (class 2606 OID 57856)
-- Name: GroupAccesses FK_GroupAccesses_Groups_GroupId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."GroupAccesses"
    ADD CONSTRAINT "FK_GroupAccesses_Groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES public."Groups"("Id") ON DELETE CASCADE;


--
-- TOC entry 3750 (class 2606 OID 57495)
-- Name: GroupLabels FK_GroupLabels_AspNetUsers_OwnerId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."GroupLabels"
    ADD CONSTRAINT "FK_GroupLabels_AspNetUsers_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3784 (class 2606 OID 57868)
-- Name: GroupMembers FK_GroupMembers_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."GroupMembers"
    ADD CONSTRAINT "FK_GroupMembers_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3785 (class 2606 OID 57873)
-- Name: GroupMembers FK_GroupMembers_Groups_GroupId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."GroupMembers"
    ADD CONSTRAINT "FK_GroupMembers_Groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES public."Groups"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3751 (class 2606 OID 57507)
-- Name: Groups FK_Groups_AspNetUsers_OwnerId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Groups"
    ADD CONSTRAINT "FK_Groups_AspNetUsers_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3786 (class 2606 OID 57885)
-- Name: LabelOnGroups FK_LabelOnGroups_GroupLabels_LabelId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."LabelOnGroups"
    ADD CONSTRAINT "FK_LabelOnGroups_GroupLabels_LabelId" FOREIGN KEY ("LabelId") REFERENCES public."GroupLabels"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3787 (class 2606 OID 57890)
-- Name: LabelOnGroups FK_LabelOnGroups_Groups_GroupId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."LabelOnGroups"
    ADD CONSTRAINT "FK_LabelOnGroups_Groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES public."Groups"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3752 (class 2606 OID 57519)
-- Name: LastVisits FK_LastVisits_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."LastVisits"
    ADD CONSTRAINT "FK_LastVisits_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3801 (class 2606 OID 58018)
-- Name: Likes FK_Likes_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Likes"
    ADD CONSTRAINT "FK_Likes_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3802 (class 2606 OID 58023)
-- Name: Likes FK_Likes_UserExerciseSubmissions_SubmissionId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Likes"
    ADD CONSTRAINT "FK_Likes_UserExerciseSubmissions_SubmissionId" FOREIGN KEY ("SubmissionId") REFERENCES public."UserExerciseSubmissions"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3803 (class 2606 OID 58035)
-- Name: ManualExerciseCheckings FK_ManualExerciseCheckings_AspNetUsers_LockedById; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ManualExerciseCheckings"
    ADD CONSTRAINT "FK_ManualExerciseCheckings_AspNetUsers_LockedById" FOREIGN KEY ("LockedById") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3804 (class 2606 OID 58040)
-- Name: ManualExerciseCheckings FK_ManualExerciseCheckings_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ManualExerciseCheckings"
    ADD CONSTRAINT "FK_ManualExerciseCheckings_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3805 (class 2606 OID 58045)
-- Name: ManualExerciseCheckings FK_ManualExerciseCheckings_UserExerciseSubmissions_SubmissionId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ManualExerciseCheckings"
    ADD CONSTRAINT "FK_ManualExerciseCheckings_UserExerciseSubmissions_SubmissionId" FOREIGN KEY ("SubmissionId") REFERENCES public."UserExerciseSubmissions"("Id") ON DELETE CASCADE;


--
-- TOC entry 3792 (class 2606 OID 57939)
-- Name: ManualQuizCheckings FK_ManualQuizCheckings_AspNetUsers_LockedById; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ManualQuizCheckings"
    ADD CONSTRAINT "FK_ManualQuizCheckings_AspNetUsers_LockedById" FOREIGN KEY ("LockedById") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3793 (class 2606 OID 57944)
-- Name: ManualQuizCheckings FK_ManualQuizCheckings_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ManualQuizCheckings"
    ADD CONSTRAINT "FK_ManualQuizCheckings_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3794 (class 2606 OID 57949)
-- Name: ManualQuizCheckings FK_ManualQuizCheckings_UserQuizSubmissions_Id; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ManualQuizCheckings"
    ADD CONSTRAINT "FK_ManualQuizCheckings_UserQuizSubmissions_Id" FOREIGN KEY ("Id") REFERENCES public."UserQuizSubmissions"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3837 (class 2606 OID 58247)
-- Name: NotificationDeliveries FK_NotificationDeliveries_NotificationTransports_NotificationT~; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."NotificationDeliveries"
    ADD CONSTRAINT "FK_NotificationDeliveries_NotificationTransports_NotificationT~" FOREIGN KEY ("NotificationTransportId") REFERENCES public."NotificationTransports"("Id") ON DELETE CASCADE;


--
-- TOC entry 3836 (class 2606 OID 58242)
-- Name: NotificationDeliveries FK_NotificationDeliveries_Notifications_NotificationId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."NotificationDeliveries"
    ADD CONSTRAINT "FK_NotificationDeliveries_Notifications_NotificationId" FOREIGN KEY ("NotificationId") REFERENCES public."Notifications"("Id") ON DELETE CASCADE;


--
-- TOC entry 3789 (class 2606 OID 57914)
-- Name: NotificationTransportSettings FK_NotificationTransportSettings_NotificationTransports_Notifi~; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."NotificationTransportSettings"
    ADD CONSTRAINT "FK_NotificationTransportSettings_NotificationTransports_Notifi~" FOREIGN KEY ("NotificationTransportId") REFERENCES public."NotificationTransports"("Id") ON DELETE CASCADE;


--
-- TOC entry 3753 (class 2606 OID 57534)
-- Name: NotificationTransports FK_NotificationTransports_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."NotificationTransports"
    ADD CONSTRAINT "FK_NotificationTransports_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3813 (class 2606 OID 58125)
-- Name: Notifications FK_Notifications_AdditionalScores_ScoreId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_AdditionalScores_ScoreId" FOREIGN KEY ("ScoreId") REFERENCES public."AdditionalScores"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3814 (class 2606 OID 58130)
-- Name: Notifications FK_Notifications_AspNetUsers_AddedUserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_AspNetUsers_AddedUserId" FOREIGN KEY ("AddedUserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3815 (class 2606 OID 58135)
-- Name: Notifications FK_Notifications_AspNetUsers_InitiatedById; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_AspNetUsers_InitiatedById" FOREIGN KEY ("InitiatedById") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3816 (class 2606 OID 58140)
-- Name: Notifications FK_Notifications_AspNetUsers_JoinedUserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_AspNetUsers_JoinedUserId" FOREIGN KEY ("JoinedUserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3817 (class 2606 OID 58145)
-- Name: Notifications FK_Notifications_AspNetUsers_LikedUserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_AspNetUsers_LikedUserId" FOREIGN KEY ("LikedUserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3818 (class 2606 OID 58150)
-- Name: Notifications FK_Notifications_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3819 (class 2606 OID 58155)
-- Name: Notifications FK_Notifications_Certificates_CertificateId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_Certificates_CertificateId" FOREIGN KEY ("CertificateId") REFERENCES public."Certificates"("Id") ON DELETE CASCADE;


--
-- TOC entry 3820 (class 2606 OID 58160)
-- Name: Notifications FK_Notifications_Comments_CommentId1; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_Comments_CommentId1" FOREIGN KEY ("CommentId1") REFERENCES public."Comments"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3821 (class 2606 OID 58165)
-- Name: Notifications FK_Notifications_Comments_ParentCommentId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_Comments_ParentCommentId" FOREIGN KEY ("ParentCommentId") REFERENCES public."Comments"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3822 (class 2606 OID 58170)
-- Name: Notifications FK_Notifications_CourseVersions_CourseVersionId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_CourseVersions_CourseVersionId" FOREIGN KEY ("CourseVersionId") REFERENCES public."CourseVersions"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3823 (class 2606 OID 58175)
-- Name: Notifications FK_Notifications_CourseVersions_UploadedPackageNotification_Co~; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_CourseVersions_UploadedPackageNotification_Co~" FOREIGN KEY ("UploadedPackageNotification_CourseVersionId") REFERENCES public."CourseVersions"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3824 (class 2606 OID 58180)
-- Name: Notifications FK_Notifications_ExerciseCodeReviewComments_CommentId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_ExerciseCodeReviewComments_CommentId" FOREIGN KEY ("CommentId") REFERENCES public."ExerciseCodeReviewComments"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3825 (class 2606 OID 58185)
-- Name: Notifications FK_Notifications_GroupAccesses_AccessId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_GroupAccesses_AccessId" FOREIGN KEY ("AccessId") REFERENCES public."GroupAccesses"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3826 (class 2606 OID 58190)
-- Name: Notifications FK_Notifications_GroupAccesses_RevokedAccessToGroupNotificatio~; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_GroupAccesses_RevokedAccessToGroupNotificatio~" FOREIGN KEY ("RevokedAccessToGroupNotification_AccessId") REFERENCES public."GroupAccesses"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3827 (class 2606 OID 58195)
-- Name: Notifications FK_Notifications_Groups_GroupId1; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_Groups_GroupId1" FOREIGN KEY ("GroupId") REFERENCES public."Groups"("Id") ON DELETE CASCADE;


--
-- TOC entry 3828 (class 2606 OID 58200)
-- Name: Notifications FK_Notifications_Groups_GroupId2; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_Groups_GroupId2" FOREIGN KEY ("GroupId") REFERENCES public."Groups"("Id") ON DELETE CASCADE;


--
-- TOC entry 3829 (class 2606 OID 58205)
-- Name: Notifications FK_Notifications_Groups_GroupId3; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_Groups_GroupId3" FOREIGN KEY ("GroupId") REFERENCES public."Groups"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3835 (class 2606 OID 58432)
-- Name: Notifications FK_Notifications_Groups_GroupId4; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_Groups_GroupId4" FOREIGN KEY ("GroupId1") REFERENCES public."Groups"("Id") ON DELETE CASCADE;


--
-- TOC entry 3830 (class 2606 OID 58210)
-- Name: Notifications FK_Notifications_Groups_GroupMemberHasBeenRemovedNotification_~; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_Groups_GroupMemberHasBeenRemovedNotification_~" FOREIGN KEY ("GroupMemberHasBeenRemovedNotification_GroupId") REFERENCES public."Groups"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3831 (class 2606 OID 58215)
-- Name: Notifications FK_Notifications_Groups_JoinedToYourGroupNotification_GroupId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_Groups_JoinedToYourGroupNotification_GroupId" FOREIGN KEY ("JoinedToYourGroupNotification_GroupId") REFERENCES public."Groups"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3832 (class 2606 OID 58220)
-- Name: Notifications FK_Notifications_ManualExerciseCheckings_CheckingId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_ManualExerciseCheckings_CheckingId" FOREIGN KEY ("CheckingId") REFERENCES public."ManualExerciseCheckings"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3833 (class 2606 OID 58225)
-- Name: Notifications FK_Notifications_ManualQuizCheckings_PassedManualQuizCheckingN~; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_ManualQuizCheckings_PassedManualQuizCheckingN~" FOREIGN KEY ("PassedManualQuizCheckingNotification_CheckingId") REFERENCES public."ManualQuizCheckings"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3834 (class 2606 OID 58230)
-- Name: Notifications FK_Notifications_StepikExportProcesses_ProcessId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_StepikExportProcesses_ProcessId" FOREIGN KEY ("ProcessId") REFERENCES public."StepikExportProcesses"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3754 (class 2606 OID 57547)
-- Name: RestoreRequests FK_RestoreRequests_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."RestoreRequests"
    ADD CONSTRAINT "FK_RestoreRequests_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3755 (class 2606 OID 57559)
-- Name: SlideHints FK_SlideHints_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."SlideHints"
    ADD CONSTRAINT "FK_SlideHints_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3756 (class 2606 OID 57571)
-- Name: SlideRates FK_SlideRates_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."SlideRates"
    ADD CONSTRAINT "FK_SlideRates_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3757 (class 2606 OID 57583)
-- Name: StepikAccessTokens FK_StepikAccessTokens_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."StepikAccessTokens"
    ADD CONSTRAINT "FK_StepikAccessTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3758 (class 2606 OID 57598)
-- Name: StepikExportProcesses FK_StepikExportProcesses_AspNetUsers_OwnerId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."StepikExportProcesses"
    ADD CONSTRAINT "FK_StepikExportProcesses_AspNetUsers_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3759 (class 2606 OID 57610)
-- Name: SystemAccesses FK_SystemAccesses_AspNetUsers_GrantedById; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."SystemAccesses"
    ADD CONSTRAINT "FK_SystemAccesses_AspNetUsers_GrantedById" FOREIGN KEY ("GrantedById") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3760 (class 2606 OID 57615)
-- Name: SystemAccesses FK_SystemAccesses_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."SystemAccesses"
    ADD CONSTRAINT "FK_SystemAccesses_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3761 (class 2606 OID 57625)
-- Name: TempCourses FK_TempCourses_AspNetUsers_AuthorId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."TempCourses"
    ADD CONSTRAINT "FK_TempCourses_AspNetUsers_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3796 (class 2606 OID 57976)
-- Name: UserExerciseSubmissions FK_UserExerciseSubmissions_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."UserExerciseSubmissions"
    ADD CONSTRAINT "FK_UserExerciseSubmissions_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3797 (class 2606 OID 57981)
-- Name: UserExerciseSubmissions FK_UserExerciseSubmissions_AutomaticExerciseCheckings_Automati~; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."UserExerciseSubmissions"
    ADD CONSTRAINT "FK_UserExerciseSubmissions_AutomaticExerciseCheckings_Automati~" FOREIGN KEY ("AutomaticCheckingId") REFERENCES public."AutomaticExerciseCheckings"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3798 (class 2606 OID 57986)
-- Name: UserExerciseSubmissions FK_UserExerciseSubmissions_TextBlobs_SolutionCodeHash; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."UserExerciseSubmissions"
    ADD CONSTRAINT "FK_UserExerciseSubmissions_TextBlobs_SolutionCodeHash" FOREIGN KEY ("SolutionCodeHash") REFERENCES public."TextBlobs"("Hash") ON DELETE CASCADE;


--
-- TOC entry 3762 (class 2606 OID 57637)
-- Name: UserFlashcardsUnlocking FK_UserFlashcardsUnlocking_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."UserFlashcardsUnlocking"
    ADD CONSTRAINT "FK_UserFlashcardsUnlocking_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3763 (class 2606 OID 57649)
-- Name: UserFlashcardsVisits FK_UserFlashcardsVisits_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."UserFlashcardsVisits"
    ADD CONSTRAINT "FK_UserFlashcardsVisits_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3764 (class 2606 OID 57664)
-- Name: UserQuestions FK_UserQuestions_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."UserQuestions"
    ADD CONSTRAINT "FK_UserQuestions_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3795 (class 2606 OID 57964)
-- Name: UserQuizAnswers FK_UserQuizAnswers_UserQuizSubmissions_SubmissionId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."UserQuizAnswers"
    ADD CONSTRAINT "FK_UserQuizAnswers_UserQuizSubmissions_SubmissionId" FOREIGN KEY ("SubmissionId") REFERENCES public."UserQuizSubmissions"("Id") ON DELETE CASCADE;


--
-- TOC entry 3765 (class 2606 OID 57676)
-- Name: UserQuizSubmissions FK_UserQuizSubmissions_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."UserQuizSubmissions"
    ADD CONSTRAINT "FK_UserQuizSubmissions_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3766 (class 2606 OID 57691)
-- Name: UserRoles FK_UserRoles_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."UserRoles"
    ADD CONSTRAINT "FK_UserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3767 (class 2606 OID 57706)
-- Name: Visits FK_Visits_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Visits"
    ADD CONSTRAINT "FK_Visits_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3806 (class 2606 OID 58060)
-- Name: XQueueExerciseSubmissions FK_XQueueExerciseSubmissions_UserExerciseSubmissions_Submissio~; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."XQueueExerciseSubmissions"
    ADD CONSTRAINT "FK_XQueueExerciseSubmissions_UserExerciseSubmissions_Submissio~" FOREIGN KEY ("SubmissionId") REFERENCES public."UserExerciseSubmissions"("Id") ON DELETE CASCADE;


--
-- TOC entry 3807 (class 2606 OID 58065)
-- Name: XQueueExerciseSubmissions FK_XQueueExerciseSubmissions_XQueueWatchers_WatcherId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."XQueueExerciseSubmissions"
    ADD CONSTRAINT "FK_XQueueExerciseSubmissions_XQueueWatchers_WatcherId" FOREIGN KEY ("WatcherId") REFERENCES public."XQueueWatchers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3768 (class 2606 OID 57721)
-- Name: XQueueWatchers FK_XQueueWatchers_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."XQueueWatchers"
    ADD CONSTRAINT "FK_XQueueWatchers_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


-- Completed on 2021-03-17 17:27:31

--
-- PostgreSQL database dump complete
--

SELECT print_notice('create indexes');

--
-- TOC entry 3372 (class 1259 OID 57209)
-- Name: IX_Clients_Token; Type: INDEX; Schema: antiplagiarism; Owner: ulearn
--

CREATE UNIQUE INDEX "IX_Clients_Token" ON antiplagiarism."Clients" USING btree ("Token");


--
-- TOC entry 3373 (class 1259 OID 57210)
-- Name: IX_Clients_Token_IsEnabled; Type: INDEX; Schema: antiplagiarism; Owner: ulearn
--

CREATE INDEX "IX_Clients_Token_IsEnabled" ON antiplagiarism."Clients" USING btree ("Token", "IsEnabled");


--
-- TOC entry 3404 (class 1259 OID 57211)
-- Name: IX_MostSimilarSubmissions_SimilarSubmissionId; Type: INDEX; Schema: antiplagiarism; Owner: ulearn
--

CREATE INDEX "IX_MostSimilarSubmissions_SimilarSubmissionId" ON antiplagiarism."MostSimilarSubmissions" USING btree ("SimilarSubmissionId");


--
-- TOC entry 3405 (class 1259 OID 57212)
-- Name: IX_MostSimilarSubmissions_Timestamp; Type: INDEX; Schema: antiplagiarism; Owner: ulearn
--

CREATE INDEX "IX_MostSimilarSubmissions_Timestamp" ON antiplagiarism."MostSimilarSubmissions" USING btree ("Timestamp");


--
-- TOC entry 3408 (class 1259 OID 57214)
-- Name: IX_SnippetsOccurences_SnippetId_SubmissionId; Type: INDEX; Schema: antiplagiarism; Owner: ulearn
--

CREATE INDEX "IX_SnippetsOccurences_SnippetId_SubmissionId" ON antiplagiarism."SnippetsOccurences" USING btree ("SnippetId", "SubmissionId");


--
-- TOC entry 3409 (class 1259 OID 57215)
-- Name: IX_SnippetsOccurences_SubmissionId_FirstTokenIndex; Type: INDEX; Schema: antiplagiarism; Owner: ulearn
--

CREATE INDEX "IX_SnippetsOccurences_SubmissionId_FirstTokenIndex" ON antiplagiarism."SnippetsOccurences" USING btree ("SubmissionId", "FirstTokenIndex");


--
-- TOC entry 3410 (class 1259 OID 57216)
-- Name: IX_SnippetsOccurences_SubmissionId_SnippetId; Type: INDEX; Schema: antiplagiarism; Owner: ulearn
--

CREATE INDEX "IX_SnippetsOccurences_SubmissionId_SnippetId" ON antiplagiarism."SnippetsOccurences" USING btree ("SubmissionId", "SnippetId");


--
-- TOC entry 3400 (class 1259 OID 57217)
-- Name: IX_SnippetsStatistics_ClientId; Type: INDEX; Schema: antiplagiarism; Owner: ulearn
--

CREATE INDEX "IX_SnippetsStatistics_ClientId" ON antiplagiarism."SnippetsStatistics" USING btree ("ClientId");


--
-- TOC entry 3401 (class 1259 OID 57218)
-- Name: IX_SnippetsStatistics_SnippetId_TaskId_ClientId; Type: INDEX; Schema: antiplagiarism; Owner: ulearn
--

CREATE UNIQUE INDEX "IX_SnippetsStatistics_SnippetId_TaskId_ClientId" ON antiplagiarism."SnippetsStatistics" USING btree ("SnippetId", "TaskId", "ClientId");


--
-- TOC entry 3382 (class 1259 OID 57213)
-- Name: IX_Snippets_TokensCount_SnippetType_Hash; Type: INDEX; Schema: antiplagiarism; Owner: ulearn
--

CREATE UNIQUE INDEX "IX_Snippets_TokensCount_SnippetType_Hash" ON antiplagiarism."Snippets" USING btree ("TokensCount", "SnippetType", "Hash");


--
-- TOC entry 3390 (class 1259 OID 57219)
-- Name: IX_Submissions_AddingTime; Type: INDEX; Schema: antiplagiarism; Owner: ulearn
--

CREATE INDEX "IX_Submissions_AddingTime" ON antiplagiarism."Submissions" USING btree ("AddingTime");


--
-- TOC entry 3391 (class 1259 OID 57220)
-- Name: IX_Submissions_ClientId_ClientSubmissionId; Type: INDEX; Schema: antiplagiarism; Owner: ulearn
--

CREATE INDEX "IX_Submissions_ClientId_ClientSubmissionId" ON antiplagiarism."Submissions" USING btree ("ClientId", "ClientSubmissionId");


--
-- TOC entry 3392 (class 1259 OID 57221)
-- Name: IX_Submissions_ClientId_TaskId; Type: INDEX; Schema: antiplagiarism; Owner: ulearn
--

CREATE INDEX "IX_Submissions_ClientId_TaskId" ON antiplagiarism."Submissions" USING btree ("ClientId", "TaskId");


--
-- TOC entry 3393 (class 1259 OID 57222)
-- Name: IX_Submissions_ClientId_TaskId_AddingTime_AuthorId; Type: INDEX; Schema: antiplagiarism; Owner: ulearn
--

CREATE INDEX "IX_Submissions_ClientId_TaskId_AddingTime_AuthorId" ON antiplagiarism."Submissions" USING btree ("ClientId", "TaskId", "AddingTime", "AuthorId");


--
-- TOC entry 3394 (class 1259 OID 57223)
-- Name: IX_Submissions_ClientId_TaskId_AddingTime_Language_AuthorId; Type: INDEX; Schema: antiplagiarism; Owner: ulearn
--

CREATE INDEX "IX_Submissions_ClientId_TaskId_AddingTime_Language_AuthorId" ON antiplagiarism."Submissions" USING btree ("ClientId", "TaskId", "AddingTime", "Language", "AuthorId");


--
-- TOC entry 3395 (class 1259 OID 57224)
-- Name: IX_Submissions_ClientId_TaskId_AuthorId; Type: INDEX; Schema: antiplagiarism; Owner: ulearn
--

CREATE INDEX "IX_Submissions_ClientId_TaskId_AuthorId" ON antiplagiarism."Submissions" USING btree ("ClientId", "TaskId", "AuthorId");


--
-- TOC entry 3396 (class 1259 OID 57225)
-- Name: IX_Submissions_ClientId_TaskId_Language_AuthorId; Type: INDEX; Schema: antiplagiarism; Owner: ulearn
--

CREATE INDEX "IX_Submissions_ClientId_TaskId_Language_AuthorId" ON antiplagiarism."Submissions" USING btree ("ClientId", "TaskId", "Language", "AuthorId");


--
-- TOC entry 3397 (class 1259 OID 57226)
-- Name: IX_Submissions_ProgramId; Type: INDEX; Schema: antiplagiarism; Owner: ulearn
--

CREATE INDEX "IX_Submissions_ProgramId" ON antiplagiarism."Submissions" USING btree ("ProgramId");


--
-- TOC entry 3413 (class 1259 OID 57227)
-- Name: IX_TaskStatisticsSourceData_Submission1Id; Type: INDEX; Schema: antiplagiarism; Owner: ulearn
--

CREATE INDEX "IX_TaskStatisticsSourceData_Submission1Id" ON antiplagiarism."TaskStatisticsSourceData" USING btree ("Submission1Id");


--
-- TOC entry 3414 (class 1259 OID 57228)
-- Name: IX_TaskStatisticsSourceData_Submission2Id; Type: INDEX; Schema: antiplagiarism; Owner: ulearn
--

CREATE INDEX "IX_TaskStatisticsSourceData_Submission2Id" ON antiplagiarism."TaskStatisticsSourceData" USING btree ("Submission2Id");


--
-- TOC entry 3387 (class 1259 OID 57229)
-- Name: IX_WorkQueueItems_QueueId_TakeAfterTime; Type: INDEX; Schema: antiplagiarism; Owner: ulearn
--

CREATE INDEX "IX_WorkQueueItems_QueueId_TakeAfterTime" ON antiplagiarism."WorkQueueItems" USING btree ("QueueId", "TakeAfterTime");


--
-- TOC entry 3420 (class 1259 OID 58262)
-- Name: EmailIndex; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "EmailIndex" ON public."AspNetUsers" USING btree ("NormalizedEmail");


--
-- TOC entry 3459 (class 1259 OID 58252)
-- Name: IX_AdditionalScores_CourseId_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AdditionalScores_CourseId_UserId" ON public."AdditionalScores" USING btree ("CourseId", "UserId");


--
-- TOC entry 3460 (class 1259 OID 58253)
-- Name: IX_AdditionalScores_CourseId_UserId_UnitId_ScoringGroupId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE UNIQUE INDEX "IX_AdditionalScores_CourseId_UserId_UnitId_ScoringGroupId" ON public."AdditionalScores" USING btree ("CourseId", "UserId", "UnitId", "ScoringGroupId");


--
-- TOC entry 3461 (class 1259 OID 58254)
-- Name: IX_AdditionalScores_InstructorId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AdditionalScores_InstructorId" ON public."AdditionalScores" USING btree ("InstructorId");


--
-- TOC entry 3462 (class 1259 OID 58255)
-- Name: IX_AdditionalScores_UnitId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AdditionalScores_UnitId" ON public."AdditionalScores" USING btree ("UnitId");


--
-- TOC entry 3463 (class 1259 OID 58256)
-- Name: IX_AdditionalScores_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AdditionalScores_UserId" ON public."AdditionalScores" USING btree ("UserId");


--
-- TOC entry 3456 (class 1259 OID 58257)
-- Name: IX_AspNetRoleClaims_RoleId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON public."AspNetRoleClaims" USING btree ("RoleId");


--
-- TOC entry 3466 (class 1259 OID 58259)
-- Name: IX_AspNetUserClaims_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AspNetUserClaims_UserId" ON public."AspNetUserClaims" USING btree ("UserId");


--
-- TOC entry 3469 (class 1259 OID 58260)
-- Name: IX_AspNetUserLogins_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AspNetUserLogins_UserId" ON public."AspNetUserLogins" USING btree ("UserId");


--
-- TOC entry 3472 (class 1259 OID 58261)
-- Name: IX_AspNetUserRoles_RoleId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AspNetUserRoles_RoleId" ON public."AspNetUserRoles" USING btree ("RoleId");


--
-- TOC entry 3421 (class 1259 OID 58263)
-- Name: IX_AspNetUsers_IsDeleted; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AspNetUsers_IsDeleted" ON public."AspNetUsers" USING btree ("IsDeleted");


--
-- TOC entry 3422 (class 1259 OID 58264)
-- Name: IX_AspNetUsers_Names; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AspNetUsers_Names" ON public."AspNetUsers" USING gin ("Names" public.gin_trgm_ops);


--
-- TOC entry 3423 (class 1259 OID 58265)
-- Name: IX_AspNetUsers_TelegramChatId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AspNetUsers_TelegramChatId" ON public."AspNetUsers" USING btree ("TelegramChatId");


--
-- TOC entry 3577 (class 1259 OID 58267)
-- Name: IX_AutomaticExerciseCheckings_CompilationErrorHash; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AutomaticExerciseCheckings_CompilationErrorHash" ON public."AutomaticExerciseCheckings" USING btree ("CompilationErrorHash");


--
-- TOC entry 3578 (class 1259 OID 58268)
-- Name: IX_AutomaticExerciseCheckings_CourseId_SlideId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AutomaticExerciseCheckings_CourseId_SlideId" ON public."AutomaticExerciseCheckings" USING btree ("CourseId", "SlideId");


--
-- TOC entry 3579 (class 1259 OID 58269)
-- Name: IX_AutomaticExerciseCheckings_CourseId_SlideId_Timestamp; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AutomaticExerciseCheckings_CourseId_SlideId_Timestamp" ON public."AutomaticExerciseCheckings" USING btree ("CourseId", "SlideId", "Timestamp");


--
-- TOC entry 3580 (class 1259 OID 58270)
-- Name: IX_AutomaticExerciseCheckings_CourseId_SlideId_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AutomaticExerciseCheckings_CourseId_SlideId_UserId" ON public."AutomaticExerciseCheckings" USING btree ("CourseId", "SlideId", "UserId");


--
-- TOC entry 3581 (class 1259 OID 58271)
-- Name: IX_AutomaticExerciseCheckings_CourseId_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AutomaticExerciseCheckings_CourseId_UserId" ON public."AutomaticExerciseCheckings" USING btree ("CourseId", "UserId");


--
-- TOC entry 3582 (class 1259 OID 58272)
-- Name: IX_AutomaticExerciseCheckings_DebugLogsHash; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AutomaticExerciseCheckings_DebugLogsHash" ON public."AutomaticExerciseCheckings" USING btree ("DebugLogsHash");


--
-- TOC entry 3583 (class 1259 OID 58273)
-- Name: IX_AutomaticExerciseCheckings_IsRightAnswer; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AutomaticExerciseCheckings_IsRightAnswer" ON public."AutomaticExerciseCheckings" USING btree ("IsRightAnswer");


--
-- TOC entry 3584 (class 1259 OID 58274)
-- Name: IX_AutomaticExerciseCheckings_OutputHash; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AutomaticExerciseCheckings_OutputHash" ON public."AutomaticExerciseCheckings" USING btree ("OutputHash");


--
-- TOC entry 3585 (class 1259 OID 58275)
-- Name: IX_AutomaticExerciseCheckings_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AutomaticExerciseCheckings_UserId" ON public."AutomaticExerciseCheckings" USING btree ("UserId");


--
-- TOC entry 3627 (class 1259 OID 58276)
-- Name: IX_AutomaticQuizCheckings_CourseId_SlideId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AutomaticQuizCheckings_CourseId_SlideId" ON public."AutomaticQuizCheckings" USING btree ("CourseId", "SlideId");


--
-- TOC entry 3628 (class 1259 OID 58277)
-- Name: IX_AutomaticQuizCheckings_CourseId_SlideId_Timestamp; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AutomaticQuizCheckings_CourseId_SlideId_Timestamp" ON public."AutomaticQuizCheckings" USING btree ("CourseId", "SlideId", "Timestamp");


--
-- TOC entry 3629 (class 1259 OID 58278)
-- Name: IX_AutomaticQuizCheckings_CourseId_SlideId_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AutomaticQuizCheckings_CourseId_SlideId_UserId" ON public."AutomaticQuizCheckings" USING btree ("CourseId", "SlideId", "UserId");


--
-- TOC entry 3630 (class 1259 OID 58279)
-- Name: IX_AutomaticQuizCheckings_CourseId_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AutomaticQuizCheckings_CourseId_UserId" ON public."AutomaticQuizCheckings" USING btree ("CourseId", "UserId");


--
-- TOC entry 3631 (class 1259 OID 58280)
-- Name: IX_AutomaticQuizCheckings_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_AutomaticQuizCheckings_UserId" ON public."AutomaticQuizCheckings" USING btree ("UserId");


--
-- TOC entry 3574 (class 1259 OID 58284)
-- Name: IX_CertificateTemplateArchives_CertificateTemplateId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_CertificateTemplateArchives_CertificateTemplateId" ON public."CertificateTemplateArchives" USING btree ("CertificateTemplateId");


--
-- TOC entry 3427 (class 1259 OID 58285)
-- Name: IX_CertificateTemplates_CourseId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_CertificateTemplates_CourseId" ON public."CertificateTemplates" USING btree ("CourseId");


--
-- TOC entry 3569 (class 1259 OID 58281)
-- Name: IX_Certificates_InstructorId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Certificates_InstructorId" ON public."Certificates" USING btree ("InstructorId");


--
-- TOC entry 3570 (class 1259 OID 58282)
-- Name: IX_Certificates_TemplateId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Certificates_TemplateId" ON public."Certificates" USING btree ("TemplateId");


--
-- TOC entry 3571 (class 1259 OID 58283)
-- Name: IX_Certificates_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Certificates_UserId" ON public."Certificates" USING btree ("UserId");


--
-- TOC entry 3588 (class 1259 OID 58286)
-- Name: IX_CommentLikes_CommentId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_CommentLikes_CommentId" ON public."CommentLikes" USING btree ("CommentId");


--
-- TOC entry 3589 (class 1259 OID 58287)
-- Name: IX_CommentLikes_UserId_CommentId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE UNIQUE INDEX "IX_CommentLikes_UserId_CommentId" ON public."CommentLikes" USING btree ("UserId", "CommentId");


--
-- TOC entry 3477 (class 1259 OID 58288)
-- Name: IX_Comments_AuthorId_PublishTime; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Comments_AuthorId_PublishTime" ON public."Comments" USING btree ("AuthorId", "PublishTime");


--
-- TOC entry 3478 (class 1259 OID 58289)
-- Name: IX_Comments_SlideId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Comments_SlideId" ON public."Comments" USING btree ("SlideId");


--
-- TOC entry 3481 (class 1259 OID 58290)
-- Name: IX_CourseAccesses_CourseId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_CourseAccesses_CourseId" ON public."CourseAccesses" USING btree ("CourseId");


--
-- TOC entry 3482 (class 1259 OID 58291)
-- Name: IX_CourseAccesses_CourseId_IsEnabled; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_CourseAccesses_CourseId_IsEnabled" ON public."CourseAccesses" USING btree ("CourseId", "IsEnabled");


--
-- TOC entry 3483 (class 1259 OID 58292)
-- Name: IX_CourseAccesses_CourseId_UserId_IsEnabled; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_CourseAccesses_CourseId_UserId_IsEnabled" ON public."CourseAccesses" USING btree ("CourseId", "UserId", "IsEnabled");


--
-- TOC entry 3484 (class 1259 OID 58294)
-- Name: IX_CourseAccesses_GrantTime; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_CourseAccesses_GrantTime" ON public."CourseAccesses" USING btree ("GrantTime");


--
-- TOC entry 3485 (class 1259 OID 58293)
-- Name: IX_CourseAccesses_GrantedById; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_CourseAccesses_GrantedById" ON public."CourseAccesses" USING btree ("GrantedById");


--
-- TOC entry 3486 (class 1259 OID 58295)
-- Name: IX_CourseAccesses_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_CourseAccesses_UserId" ON public."CourseAccesses" USING btree ("UserId");


--
-- TOC entry 3592 (class 1259 OID 58296)
-- Name: IX_CourseFiles_CourseVersionId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_CourseFiles_CourseVersionId" ON public."CourseFiles" USING btree ("CourseVersionId");


--
-- TOC entry 3489 (class 1259 OID 58297)
-- Name: IX_CourseVersions_AuthorId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_CourseVersions_AuthorId" ON public."CourseVersions" USING btree ("AuthorId");


--
-- TOC entry 3490 (class 1259 OID 58298)
-- Name: IX_CourseVersions_CourseId_LoadingTime; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_CourseVersions_CourseId_LoadingTime" ON public."CourseVersions" USING btree ("CourseId", "LoadingTime");


--
-- TOC entry 3491 (class 1259 OID 58299)
-- Name: IX_CourseVersions_CourseId_PublishTime; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_CourseVersions_CourseId_PublishTime" ON public."CourseVersions" USING btree ("CourseId", "PublishTime");


--
-- TOC entry 3595 (class 1259 OID 58300)
-- Name: IX_EnabledAdditionalScoringGroups_GroupId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_EnabledAdditionalScoringGroups_GroupId" ON public."EnabledAdditionalScoringGroups" USING btree ("GroupId");


--
-- TOC entry 3687 (class 1259 OID 58301)
-- Name: IX_ExerciseCodeReviewComments_AddingTime; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_ExerciseCodeReviewComments_AddingTime" ON public."ExerciseCodeReviewComments" USING btree ("AddingTime");


--
-- TOC entry 3688 (class 1259 OID 58302)
-- Name: IX_ExerciseCodeReviewComments_AuthorId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_ExerciseCodeReviewComments_AuthorId" ON public."ExerciseCodeReviewComments" USING btree ("AuthorId");


--
-- TOC entry 3689 (class 1259 OID 58303)
-- Name: IX_ExerciseCodeReviewComments_ReviewId_IsDeleted; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_ExerciseCodeReviewComments_ReviewId_IsDeleted" ON public."ExerciseCodeReviewComments" USING btree ("ReviewId", "IsDeleted");


--
-- TOC entry 3682 (class 1259 OID 58304)
-- Name: IX_ExerciseCodeReviews_AuthorId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_ExerciseCodeReviews_AuthorId" ON public."ExerciseCodeReviews" USING btree ("AuthorId");


--
-- TOC entry 3683 (class 1259 OID 58305)
-- Name: IX_ExerciseCodeReviews_ExerciseCheckingId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_ExerciseCodeReviews_ExerciseCheckingId" ON public."ExerciseCodeReviews" USING btree ("ExerciseCheckingId");


--
-- TOC entry 3684 (class 1259 OID 58306)
-- Name: IX_ExerciseCodeReviews_SubmissionId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_ExerciseCodeReviews_SubmissionId" ON public."ExerciseCodeReviews" USING btree ("SubmissionId");


--
-- TOC entry 3661 (class 1259 OID 58307)
-- Name: IX_ExerciseSolutionByGraders_ClientId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_ExerciseSolutionByGraders_ClientId" ON public."ExerciseSolutionByGraders" USING btree ("ClientId");


--
-- TOC entry 3662 (class 1259 OID 58308)
-- Name: IX_ExerciseSolutionByGraders_SubmissionId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_ExerciseSolutionByGraders_SubmissionId" ON public."ExerciseSolutionByGraders" USING btree ("SubmissionId");


--
-- TOC entry 3615 (class 1259 OID 58309)
-- Name: IX_FeedViewTimestamps_Timestamp; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_FeedViewTimestamps_Timestamp" ON public."FeedViewTimestamps" USING btree ("Timestamp");


--
-- TOC entry 3616 (class 1259 OID 58310)
-- Name: IX_FeedViewTimestamps_TransportId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_FeedViewTimestamps_TransportId" ON public."FeedViewTimestamps" USING btree ("TransportId");


--
-- TOC entry 3617 (class 1259 OID 58311)
-- Name: IX_FeedViewTimestamps_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_FeedViewTimestamps_UserId" ON public."FeedViewTimestamps" USING btree ("UserId");


--
-- TOC entry 3618 (class 1259 OID 58312)
-- Name: IX_FeedViewTimestamps_UserId_TransportId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_FeedViewTimestamps_UserId_TransportId" ON public."FeedViewTimestamps" USING btree ("UserId", "TransportId");


--
-- TOC entry 3494 (class 1259 OID 58313)
-- Name: IX_GraderClients_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_GraderClients_UserId" ON public."GraderClients" USING btree ("UserId");


--
-- TOC entry 3598 (class 1259 OID 58315)
-- Name: IX_GroupAccesses_GrantTime; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_GroupAccesses_GrantTime" ON public."GroupAccesses" USING btree ("GrantTime");


--
-- TOC entry 3599 (class 1259 OID 58314)
-- Name: IX_GroupAccesses_GrantedById; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_GroupAccesses_GrantedById" ON public."GroupAccesses" USING btree ("GrantedById");


--
-- TOC entry 3600 (class 1259 OID 58316)
-- Name: IX_GroupAccesses_GroupId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_GroupAccesses_GroupId" ON public."GroupAccesses" USING btree ("GroupId");


--
-- TOC entry 3601 (class 1259 OID 58317)
-- Name: IX_GroupAccesses_GroupId_IsEnabled; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_GroupAccesses_GroupId_IsEnabled" ON public."GroupAccesses" USING btree ("GroupId", "IsEnabled");


--
-- TOC entry 3602 (class 1259 OID 58318)
-- Name: IX_GroupAccesses_GroupId_UserId_IsEnabled; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_GroupAccesses_GroupId_UserId_IsEnabled" ON public."GroupAccesses" USING btree ("GroupId", "UserId", "IsEnabled");


--
-- TOC entry 3603 (class 1259 OID 58319)
-- Name: IX_GroupAccesses_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_GroupAccesses_UserId" ON public."GroupAccesses" USING btree ("UserId");


--
-- TOC entry 3497 (class 1259 OID 58320)
-- Name: IX_GroupLabels_OwnerId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_GroupLabels_OwnerId" ON public."GroupLabels" USING btree ("OwnerId");


--
-- TOC entry 3498 (class 1259 OID 58321)
-- Name: IX_GroupLabels_OwnerId_IsDeleted; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_GroupLabels_OwnerId_IsDeleted" ON public."GroupLabels" USING btree ("OwnerId", "IsDeleted");


--
-- TOC entry 3606 (class 1259 OID 58322)
-- Name: IX_GroupMembers_GroupId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_GroupMembers_GroupId" ON public."GroupMembers" USING btree ("GroupId");


--
-- TOC entry 3607 (class 1259 OID 58323)
-- Name: IX_GroupMembers_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_GroupMembers_UserId" ON public."GroupMembers" USING btree ("UserId");


--
-- TOC entry 3501 (class 1259 OID 58324)
-- Name: IX_Groups_CourseId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Groups_CourseId" ON public."Groups" USING btree ("CourseId");


--
-- TOC entry 3502 (class 1259 OID 58325)
-- Name: IX_Groups_InviteHash; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Groups_InviteHash" ON public."Groups" USING btree ("InviteHash");


--
-- TOC entry 3503 (class 1259 OID 58326)
-- Name: IX_Groups_OwnerId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Groups_OwnerId" ON public."Groups" USING btree ("OwnerId");


--
-- TOC entry 3610 (class 1259 OID 58327)
-- Name: IX_LabelOnGroups_GroupId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_LabelOnGroups_GroupId" ON public."LabelOnGroups" USING btree ("GroupId");


--
-- TOC entry 3611 (class 1259 OID 58328)
-- Name: IX_LabelOnGroups_GroupId_LabelId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE UNIQUE INDEX "IX_LabelOnGroups_GroupId_LabelId" ON public."LabelOnGroups" USING btree ("GroupId", "LabelId");


--
-- TOC entry 3612 (class 1259 OID 58329)
-- Name: IX_LabelOnGroups_LabelId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_LabelOnGroups_LabelId" ON public."LabelOnGroups" USING btree ("LabelId");


--
-- TOC entry 3506 (class 1259 OID 58330)
-- Name: IX_LastVisits_CourseId_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_LastVisits_CourseId_UserId" ON public."LastVisits" USING btree ("CourseId", "UserId");


--
-- TOC entry 3507 (class 1259 OID 58331)
-- Name: IX_LastVisits_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_LastVisits_UserId" ON public."LastVisits" USING btree ("UserId");


--
-- TOC entry 3665 (class 1259 OID 58332)
-- Name: IX_Likes_SubmissionId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Likes_SubmissionId" ON public."Likes" USING btree ("SubmissionId");


--
-- TOC entry 3666 (class 1259 OID 58333)
-- Name: IX_Likes_UserId_SubmissionId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Likes_UserId_SubmissionId" ON public."Likes" USING btree ("UserId", "SubmissionId");


--
-- TOC entry 3434 (class 1259 OID 58334)
-- Name: IX_LtiConsumers_Key; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_LtiConsumers_Key" ON public."LtiConsumers" USING btree ("Key");


--
-- TOC entry 3437 (class 1259 OID 58335)
-- Name: IX_LtiSlideRequests_CourseId_SlideId_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_LtiSlideRequests_CourseId_SlideId_UserId" ON public."LtiSlideRequests" USING btree ("CourseId", "SlideId", "UserId");


--
-- TOC entry 3669 (class 1259 OID 58336)
-- Name: IX_ManualExerciseCheckings_CourseId_SlideId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_ManualExerciseCheckings_CourseId_SlideId" ON public."ManualExerciseCheckings" USING btree ("CourseId", "SlideId");


--
-- TOC entry 3670 (class 1259 OID 58337)
-- Name: IX_ManualExerciseCheckings_CourseId_SlideId_Timestamp; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_ManualExerciseCheckings_CourseId_SlideId_Timestamp" ON public."ManualExerciseCheckings" USING btree ("CourseId", "SlideId", "Timestamp");


--
-- TOC entry 3671 (class 1259 OID 58338)
-- Name: IX_ManualExerciseCheckings_CourseId_SlideId_UserId_ProhibitFur~; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_ManualExerciseCheckings_CourseId_SlideId_UserId_ProhibitFur~" ON public."ManualExerciseCheckings" USING btree ("CourseId", "SlideId", "UserId", "ProhibitFurtherManualCheckings");


--
-- TOC entry 3672 (class 1259 OID 58339)
-- Name: IX_ManualExerciseCheckings_CourseId_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_ManualExerciseCheckings_CourseId_UserId" ON public."ManualExerciseCheckings" USING btree ("CourseId", "UserId");


--
-- TOC entry 3673 (class 1259 OID 58340)
-- Name: IX_ManualExerciseCheckings_LockedById; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_ManualExerciseCheckings_LockedById" ON public."ManualExerciseCheckings" USING btree ("LockedById");


--
-- TOC entry 3674 (class 1259 OID 58341)
-- Name: IX_ManualExerciseCheckings_SubmissionId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_ManualExerciseCheckings_SubmissionId" ON public."ManualExerciseCheckings" USING btree ("SubmissionId");


--
-- TOC entry 3675 (class 1259 OID 58342)
-- Name: IX_ManualExerciseCheckings_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_ManualExerciseCheckings_UserId" ON public."ManualExerciseCheckings" USING btree ("UserId");


--
-- TOC entry 3634 (class 1259 OID 58343)
-- Name: IX_ManualQuizCheckings_CourseId_SlideId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_ManualQuizCheckings_CourseId_SlideId" ON public."ManualQuizCheckings" USING btree ("CourseId", "SlideId");


--
-- TOC entry 3635 (class 1259 OID 58344)
-- Name: IX_ManualQuizCheckings_CourseId_SlideId_Timestamp; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_ManualQuizCheckings_CourseId_SlideId_Timestamp" ON public."ManualQuizCheckings" USING btree ("CourseId", "SlideId", "Timestamp");


--
-- TOC entry 3636 (class 1259 OID 58345)
-- Name: IX_ManualQuizCheckings_CourseId_SlideId_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_ManualQuizCheckings_CourseId_SlideId_UserId" ON public."ManualQuizCheckings" USING btree ("CourseId", "SlideId", "UserId");


--
-- TOC entry 3637 (class 1259 OID 58346)
-- Name: IX_ManualQuizCheckings_CourseId_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_ManualQuizCheckings_CourseId_UserId" ON public."ManualQuizCheckings" USING btree ("CourseId", "UserId");


--
-- TOC entry 3638 (class 1259 OID 58347)
-- Name: IX_ManualQuizCheckings_LockedById; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_ManualQuizCheckings_LockedById" ON public."ManualQuizCheckings" USING btree ("LockedById");


--
-- TOC entry 3639 (class 1259 OID 58348)
-- Name: IX_ManualQuizCheckings_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_ManualQuizCheckings_UserId" ON public."ManualQuizCheckings" USING btree ("UserId");


--
-- TOC entry 3719 (class 1259 OID 58349)
-- Name: IX_NotificationDeliveries_CreateTime; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_NotificationDeliveries_CreateTime" ON public."NotificationDeliveries" USING btree ("CreateTime");


--
-- TOC entry 3720 (class 1259 OID 58350)
-- Name: IX_NotificationDeliveries_NextTryTime; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_NotificationDeliveries_NextTryTime" ON public."NotificationDeliveries" USING btree ("NextTryTime");


--
-- TOC entry 3721 (class 1259 OID 58351)
-- Name: IX_NotificationDeliveries_NotificationId_NotificationTransport~; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_NotificationDeliveries_NotificationId_NotificationTransport~" ON public."NotificationDeliveries" USING btree ("NotificationId", "NotificationTransportId");


--
-- TOC entry 3722 (class 1259 OID 58352)
-- Name: IX_NotificationDeliveries_NotificationTransportId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_NotificationDeliveries_NotificationTransportId" ON public."NotificationDeliveries" USING btree ("NotificationTransportId");


--
-- TOC entry 3621 (class 1259 OID 58380)
-- Name: IX_NotificationTransportSettings_CourseId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_NotificationTransportSettings_CourseId" ON public."NotificationTransportSettings" USING btree ("CourseId");


--
-- TOC entry 3622 (class 1259 OID 58381)
-- Name: IX_NotificationTransportSettings_CourseId_NotificationType; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_NotificationTransportSettings_CourseId_NotificationType" ON public."NotificationTransportSettings" USING btree ("CourseId", "NotificationType");


--
-- TOC entry 3623 (class 1259 OID 58382)
-- Name: IX_NotificationTransportSettings_NotificationTransportId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_NotificationTransportSettings_NotificationTransportId" ON public."NotificationTransportSettings" USING btree ("NotificationTransportId");


--
-- TOC entry 3624 (class 1259 OID 58383)
-- Name: IX_NotificationTransportSettings_NotificationType; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_NotificationTransportSettings_NotificationType" ON public."NotificationTransportSettings" USING btree ("NotificationType");


--
-- TOC entry 3510 (class 1259 OID 58378)
-- Name: IX_NotificationTransports_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_NotificationTransports_UserId" ON public."NotificationTransports" USING btree ("UserId");


--
-- TOC entry 3511 (class 1259 OID 58379)
-- Name: IX_NotificationTransports_UserId_IsDeleted; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_NotificationTransports_UserId_IsDeleted" ON public."NotificationTransports" USING btree ("UserId", "IsDeleted");


--
-- TOC entry 3692 (class 1259 OID 58353)
-- Name: IX_Notifications_AccessId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_AccessId" ON public."Notifications" USING btree ("AccessId");


--
-- TOC entry 3693 (class 1259 OID 58354)
-- Name: IX_Notifications_AddedUserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_AddedUserId" ON public."Notifications" USING btree ("AddedUserId");


--
-- TOC entry 3694 (class 1259 OID 58355)
-- Name: IX_Notifications_AreDeliveriesCreated; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_AreDeliveriesCreated" ON public."Notifications" USING btree ("AreDeliveriesCreated");


--
-- TOC entry 3695 (class 1259 OID 58356)
-- Name: IX_Notifications_CertificateId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_CertificateId" ON public."Notifications" USING btree ("CertificateId");


--
-- TOC entry 3696 (class 1259 OID 58357)
-- Name: IX_Notifications_CheckingId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_CheckingId" ON public."Notifications" USING btree ("CheckingId");


--
-- TOC entry 3697 (class 1259 OID 58358)
-- Name: IX_Notifications_CommentId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_CommentId" ON public."Notifications" USING btree ("CommentId");


--
-- TOC entry 3698 (class 1259 OID 58359)
-- Name: IX_Notifications_CommentId1; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_CommentId1" ON public."Notifications" USING btree ("CommentId1");


--
-- TOC entry 3699 (class 1259 OID 58360)
-- Name: IX_Notifications_CourseId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_CourseId" ON public."Notifications" USING btree ("CourseId");


--
-- TOC entry 3700 (class 1259 OID 58361)
-- Name: IX_Notifications_CourseVersionId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_CourseVersionId" ON public."Notifications" USING btree ("CourseVersionId");


--
-- TOC entry 3701 (class 1259 OID 58362)
-- Name: IX_Notifications_CreateTime; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_CreateTime" ON public."Notifications" USING btree ("CreateTime");


--
-- TOC entry 3702 (class 1259 OID 58363)
-- Name: IX_Notifications_GroupId1; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_GroupId1" ON public."Notifications" USING btree ("GroupId");


--
-- TOC entry 3703 (class 1259 OID 58364)
-- Name: IX_Notifications_GroupId2; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_GroupId2" ON public."Notifications" USING btree ("GroupId");


--
-- TOC entry 3704 (class 1259 OID 58365)
-- Name: IX_Notifications_GroupId3; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_GroupId3" ON public."Notifications" USING btree ("GroupId");


--
-- TOC entry 3705 (class 1259 OID 58366)
-- Name: IX_Notifications_GroupMemberHasBeenRemovedNotification_GroupId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_GroupMemberHasBeenRemovedNotification_GroupId" ON public."Notifications" USING btree ("GroupMemberHasBeenRemovedNotification_GroupId");


--
-- TOC entry 3706 (class 1259 OID 58367)
-- Name: IX_Notifications_InitiatedById; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_InitiatedById" ON public."Notifications" USING btree ("InitiatedById");


--
-- TOC entry 3707 (class 1259 OID 58368)
-- Name: IX_Notifications_JoinedToYourGroupNotification_GroupId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_JoinedToYourGroupNotification_GroupId" ON public."Notifications" USING btree ("JoinedToYourGroupNotification_GroupId");


--
-- TOC entry 3708 (class 1259 OID 58369)
-- Name: IX_Notifications_JoinedUserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_JoinedUserId" ON public."Notifications" USING btree ("JoinedUserId");


--
-- TOC entry 3709 (class 1259 OID 58370)
-- Name: IX_Notifications_LikedUserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_LikedUserId" ON public."Notifications" USING btree ("LikedUserId");


--
-- TOC entry 3710 (class 1259 OID 58371)
-- Name: IX_Notifications_ParentCommentId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_ParentCommentId" ON public."Notifications" USING btree ("ParentCommentId");


--
-- TOC entry 3711 (class 1259 OID 58372)
-- Name: IX_Notifications_PassedManualQuizCheckingNotification_Checking~; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_PassedManualQuizCheckingNotification_Checking~" ON public."Notifications" USING btree ("PassedManualQuizCheckingNotification_CheckingId");


--
-- TOC entry 3712 (class 1259 OID 58373)
-- Name: IX_Notifications_ProcessId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_ProcessId" ON public."Notifications" USING btree ("ProcessId");


--
-- TOC entry 3713 (class 1259 OID 58374)
-- Name: IX_Notifications_RevokedAccessToGroupNotification_AccessId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_RevokedAccessToGroupNotification_AccessId" ON public."Notifications" USING btree ("RevokedAccessToGroupNotification_AccessId");


--
-- TOC entry 3714 (class 1259 OID 58375)
-- Name: IX_Notifications_ScoreId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_ScoreId" ON public."Notifications" USING btree ("ScoreId");


--
-- TOC entry 3715 (class 1259 OID 58376)
-- Name: IX_Notifications_UploadedPackageNotification_CourseVersionId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_UploadedPackageNotification_CourseVersionId" ON public."Notifications" USING btree ("UploadedPackageNotification_CourseVersionId");


--
-- TOC entry 3716 (class 1259 OID 58377)
-- Name: IX_Notifications_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Notifications_UserId" ON public."Notifications" USING btree ("UserId");


--
-- TOC entry 3514 (class 1259 OID 58384)
-- Name: IX_RestoreRequests_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_RestoreRequests_UserId" ON public."RestoreRequests" USING btree ("UserId");


--
-- TOC entry 3517 (class 1259 OID 58385)
-- Name: IX_SlideHints_CourseId_SlideId_HintId_UserId_IsHintHelped; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_SlideHints_CourseId_SlideId_HintId_UserId_IsHintHelped" ON public."SlideHints" USING btree ("CourseId", "SlideId", "HintId", "UserId", "IsHintHelped");


--
-- TOC entry 3518 (class 1259 OID 58386)
-- Name: IX_SlideHints_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_SlideHints_UserId" ON public."SlideHints" USING btree ("UserId");


--
-- TOC entry 3521 (class 1259 OID 58387)
-- Name: IX_SlideRates_SlideId_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_SlideRates_SlideId_UserId" ON public."SlideRates" USING btree ("SlideId", "UserId");


--
-- TOC entry 3522 (class 1259 OID 58388)
-- Name: IX_SlideRates_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_SlideRates_UserId" ON public."SlideRates" USING btree ("UserId");


--
-- TOC entry 3525 (class 1259 OID 58389)
-- Name: IX_StepikAccessTokens_AddedTime; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_StepikAccessTokens_AddedTime" ON public."StepikAccessTokens" USING btree ("AddedTime");


--
-- TOC entry 3526 (class 1259 OID 58390)
-- Name: IX_StepikAccessTokens_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_StepikAccessTokens_UserId" ON public."StepikAccessTokens" USING btree ("UserId");


--
-- TOC entry 3529 (class 1259 OID 58391)
-- Name: IX_StepikExportProcesses_OwnerId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_StepikExportProcesses_OwnerId" ON public."StepikExportProcesses" USING btree ("OwnerId");


--
-- TOC entry 3440 (class 1259 OID 58392)
-- Name: IX_StepikExportSlideAndStepMaps_UlearnCourseId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_StepikExportSlideAndStepMaps_UlearnCourseId" ON public."StepikExportSlideAndStepMaps" USING btree ("UlearnCourseId");


--
-- TOC entry 3441 (class 1259 OID 58393)
-- Name: IX_StepikExportSlideAndStepMaps_UlearnCourseId_SlideId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_StepikExportSlideAndStepMaps_UlearnCourseId_SlideId" ON public."StepikExportSlideAndStepMaps" USING btree ("UlearnCourseId", "SlideId");


--
-- TOC entry 3442 (class 1259 OID 58394)
-- Name: IX_StepikExportSlideAndStepMaps_UlearnCourseId_StepikCourseId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_StepikExportSlideAndStepMaps_UlearnCourseId_StepikCourseId" ON public."StepikExportSlideAndStepMaps" USING btree ("UlearnCourseId", "StepikCourseId");


--
-- TOC entry 3532 (class 1259 OID 58396)
-- Name: IX_SystemAccesses_GrantTime; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_SystemAccesses_GrantTime" ON public."SystemAccesses" USING btree ("GrantTime");


--
-- TOC entry 3533 (class 1259 OID 58395)
-- Name: IX_SystemAccesses_GrantedById; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_SystemAccesses_GrantedById" ON public."SystemAccesses" USING btree ("GrantedById");


--
-- TOC entry 3534 (class 1259 OID 58397)
-- Name: IX_SystemAccesses_IsEnabled; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_SystemAccesses_IsEnabled" ON public."SystemAccesses" USING btree ("IsEnabled");


--
-- TOC entry 3535 (class 1259 OID 58398)
-- Name: IX_SystemAccesses_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_SystemAccesses_UserId" ON public."SystemAccesses" USING btree ("UserId");


--
-- TOC entry 3536 (class 1259 OID 58399)
-- Name: IX_SystemAccesses_UserId_IsEnabled; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_SystemAccesses_UserId_IsEnabled" ON public."SystemAccesses" USING btree ("UserId", "IsEnabled");


--
-- TOC entry 3539 (class 1259 OID 58400)
-- Name: IX_TempCourses_AuthorId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_TempCourses_AuthorId" ON public."TempCourses" USING btree ("AuthorId");


--
-- TOC entry 3451 (class 1259 OID 58401)
-- Name: IX_UnitAppearances_CourseId_PublishTime; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UnitAppearances_CourseId_PublishTime" ON public."UnitAppearances" USING btree ("CourseId", "PublishTime");


--
-- TOC entry 3646 (class 1259 OID 58402)
-- Name: IX_UserExerciseSubmissions_AntiPlagiarismSubmissionId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UserExerciseSubmissions_AntiPlagiarismSubmissionId" ON public."UserExerciseSubmissions" USING btree ("AntiPlagiarismSubmissionId");


--
-- TOC entry 3647 (class 1259 OID 58403)
-- Name: IX_UserExerciseSubmissions_AutomaticCheckingId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UserExerciseSubmissions_AutomaticCheckingId" ON public."UserExerciseSubmissions" USING btree ("AutomaticCheckingId");


--
-- TOC entry 3648 (class 1259 OID 58404)
-- Name: IX_UserExerciseSubmissions_AutomaticCheckingIsRightAnswer; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UserExerciseSubmissions_AutomaticCheckingIsRightAnswer" ON public."UserExerciseSubmissions" USING btree ("AutomaticCheckingIsRightAnswer");


--
-- TOC entry 3649 (class 1259 OID 58405)
-- Name: IX_UserExerciseSubmissions_CourseId_AutomaticCheckingIsRightAn~; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UserExerciseSubmissions_CourseId_AutomaticCheckingIsRightAn~" ON public."UserExerciseSubmissions" USING btree ("CourseId", "AutomaticCheckingIsRightAnswer");


--
-- TOC entry 3650 (class 1259 OID 58406)
-- Name: IX_UserExerciseSubmissions_CourseId_SlideId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UserExerciseSubmissions_CourseId_SlideId" ON public."UserExerciseSubmissions" USING btree ("CourseId", "SlideId");


--
-- TOC entry 3651 (class 1259 OID 58407)
-- Name: IX_UserExerciseSubmissions_CourseId_SlideId_AutomaticCheckingI~; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UserExerciseSubmissions_CourseId_SlideId_AutomaticCheckingI~" ON public."UserExerciseSubmissions" USING btree ("CourseId", "SlideId", "AutomaticCheckingIsRightAnswer");


--
-- TOC entry 3652 (class 1259 OID 58408)
-- Name: IX_UserExerciseSubmissions_CourseId_SlideId_Timestamp; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UserExerciseSubmissions_CourseId_SlideId_Timestamp" ON public."UserExerciseSubmissions" USING btree ("CourseId", "SlideId", "Timestamp");


--
-- TOC entry 3653 (class 1259 OID 58409)
-- Name: IX_UserExerciseSubmissions_CourseId_SlideId_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UserExerciseSubmissions_CourseId_SlideId_UserId" ON public."UserExerciseSubmissions" USING btree ("CourseId", "SlideId", "UserId");


--
-- TOC entry 3654 (class 1259 OID 58410)
-- Name: IX_UserExerciseSubmissions_Language; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UserExerciseSubmissions_Language" ON public."UserExerciseSubmissions" USING btree ("Language");


--
-- TOC entry 3655 (class 1259 OID 58411)
-- Name: IX_UserExerciseSubmissions_Sandbox; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UserExerciseSubmissions_Sandbox" ON public."UserExerciseSubmissions" USING btree ("Sandbox");


--
-- TOC entry 3656 (class 1259 OID 58412)
-- Name: IX_UserExerciseSubmissions_SolutionCodeHash; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UserExerciseSubmissions_SolutionCodeHash" ON public."UserExerciseSubmissions" USING btree ("SolutionCodeHash");


--
-- TOC entry 3657 (class 1259 OID 58413)
-- Name: IX_UserExerciseSubmissions_Timestamp; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UserExerciseSubmissions_Timestamp" ON public."UserExerciseSubmissions" USING btree ("Timestamp");


--
-- TOC entry 3658 (class 1259 OID 58414)
-- Name: IX_UserExerciseSubmissions_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UserExerciseSubmissions_UserId" ON public."UserExerciseSubmissions" USING btree ("UserId");


--
-- TOC entry 3542 (class 1259 OID 58415)
-- Name: IX_UserFlashcardsUnlocking_UserId_CourseId_UnitId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UserFlashcardsUnlocking_UserId_CourseId_UnitId" ON public."UserFlashcardsUnlocking" USING btree ("UserId", "CourseId", "UnitId");


--
-- TOC entry 3545 (class 1259 OID 58416)
-- Name: IX_UserFlashcardsVisits_UserId_CourseId_UnitId_FlashcardId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UserFlashcardsVisits_UserId_CourseId_UnitId_FlashcardId" ON public."UserFlashcardsVisits" USING btree ("UserId", "CourseId", "UnitId", "FlashcardId");


--
-- TOC entry 3548 (class 1259 OID 58417)
-- Name: IX_UserQuestions_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UserQuestions_UserId" ON public."UserQuestions" USING btree ("UserId");


--
-- TOC entry 3642 (class 1259 OID 58418)
-- Name: IX_UserQuizAnswers_ItemId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UserQuizAnswers_ItemId" ON public."UserQuizAnswers" USING btree ("ItemId");


--
-- TOC entry 3643 (class 1259 OID 58419)
-- Name: IX_UserQuizAnswers_SubmissionId_BlockId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UserQuizAnswers_SubmissionId_BlockId" ON public."UserQuizAnswers" USING btree ("SubmissionId", "BlockId");


--
-- TOC entry 3551 (class 1259 OID 58420)
-- Name: IX_UserQuizSubmissions_CourseId_SlideId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UserQuizSubmissions_CourseId_SlideId" ON public."UserQuizSubmissions" USING btree ("CourseId", "SlideId");


--
-- TOC entry 3552 (class 1259 OID 58421)
-- Name: IX_UserQuizSubmissions_CourseId_SlideId_Timestamp; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UserQuizSubmissions_CourseId_SlideId_Timestamp" ON public."UserQuizSubmissions" USING btree ("CourseId", "SlideId", "Timestamp");


--
-- TOC entry 3553 (class 1259 OID 58422)
-- Name: IX_UserQuizSubmissions_CourseId_SlideId_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UserQuizSubmissions_CourseId_SlideId_UserId" ON public."UserQuizSubmissions" USING btree ("CourseId", "SlideId", "UserId");


--
-- TOC entry 3554 (class 1259 OID 58423)
-- Name: IX_UserQuizSubmissions_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UserQuizSubmissions_UserId" ON public."UserQuizSubmissions" USING btree ("UserId");


--
-- TOC entry 3557 (class 1259 OID 58424)
-- Name: IX_UserRoles_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_UserRoles_UserId" ON public."UserRoles" USING btree ("UserId");


--
-- TOC entry 3560 (class 1259 OID 58425)
-- Name: IX_Visits_CourseId_SlideId_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Visits_CourseId_SlideId_UserId" ON public."Visits" USING btree ("CourseId", "SlideId", "UserId");


--
-- TOC entry 3561 (class 1259 OID 58426)
-- Name: IX_Visits_SlideId_Timestamp; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Visits_SlideId_Timestamp" ON public."Visits" USING btree ("SlideId", "Timestamp");


--
-- TOC entry 3562 (class 1259 OID 58427)
-- Name: IX_Visits_SlideId_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Visits_SlideId_UserId" ON public."Visits" USING btree ("SlideId", "UserId");


--
-- TOC entry 3563 (class 1259 OID 58428)
-- Name: IX_Visits_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Visits_UserId" ON public."Visits" USING btree ("UserId");


--
-- Name: IX_Visits_CourseId_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_Visits_CourseId_UserId" ON public."Visits" USING btree ("CourseId", "UserId");


--
-- TOC entry 3678 (class 1259 OID 58429)
-- Name: IX_XQueueExerciseSubmissions_SubmissionId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_XQueueExerciseSubmissions_SubmissionId" ON public."XQueueExerciseSubmissions" USING btree ("SubmissionId");


--
-- TOC entry 3679 (class 1259 OID 58430)
-- Name: IX_XQueueExerciseSubmissions_WatcherId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_XQueueExerciseSubmissions_WatcherId" ON public."XQueueExerciseSubmissions" USING btree ("WatcherId");


--
-- TOC entry 3566 (class 1259 OID 58431)
-- Name: IX_XQueueWatchers_UserId; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE INDEX "IX_XQueueWatchers_UserId" ON public."XQueueWatchers" USING btree ("UserId");


--
-- TOC entry 3419 (class 1259 OID 58258)
-- Name: RoleNameIndex; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE UNIQUE INDEX "RoleNameIndex" ON public."AspNetRoles" USING btree ("NormalizedName");


--
-- TOC entry 3426 (class 1259 OID 58266)
-- Name: UserNameIndex; Type: INDEX; Schema: public; Owner: ulearn
--

CREATE UNIQUE INDEX "UserNameIndex" ON public."AspNetUsers" USING btree ("NormalizedUserName");

SELECT print_notice('fill migrations');

TRUNCATE TABLE public."__EFMigrationsHistory";

INSERT INTO public."__EFMigrationsHistory"
    ("MigrationId", "ProductVersion")
VALUES
('20210319103352_Init', '5.0.3'),
('20210319103454_InitAntiPlagiarism', '5.0.3'),
('20210319105825_AddDiffFromDatabase', '5.0.3'),
('20210330093143_AddCourseUserIndexToVisits', '5.0.3')
;

SELECT print_notice('analize');

ANALYZE;