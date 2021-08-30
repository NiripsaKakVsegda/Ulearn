using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Database.Repos
{
	public class CertificatesRepo : ICertificatesRepo
	{
		private readonly UlearnDb db;

		public CertificatesRepo(UlearnDb db)
		{
			this.db = db;
		}

		public async Task<List<CertificateTemplate>> GetTemplates(string courseId)
		{
			return await db.CertificateTemplates.Where(t => t.CourseId == courseId && !t.IsDeleted).ToListAsync();
		}

		public async Task<CertificateTemplate> FindTemplateById(Guid id)
		{
			return await db.CertificateTemplates.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
		}

		public async Task<Certificate> FindCertificateById(Guid id)
		{
			return await db.Certificates.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
		}

		public async Task<List<Certificate>> GetTemplateCertificates(Guid templateId)
		{
			return await db.Certificates.Where(c => c.TemplateId == templateId && !c.IsDeleted).ToListAsync();
		}

		public async Task<CertificateTemplate> AddTemplate(string courseId, string name, string archiveName)
		{
			var template = new CertificateTemplate
			{
				Id = Guid.NewGuid(),
				CourseId = courseId,
				Name = name,
				Timestamp = DateTime.Now,
				ArchiveName = archiveName,
			};
			db.CertificateTemplates.Add(template);
			await db.SaveChangesAsync();
			return template;
		}

		public async Task<Certificate> AddCertificate(Guid templateId, string userId, string instructorId, Dictionary<string, string> parameters, bool isPreview = false)
		{
			var certificate = new Certificate
			{
				Id = Guid.NewGuid(),
				TemplateId = templateId,
				UserId = userId,
				InstructorId = instructorId,
				Parameters = JsonConvert.SerializeObject(parameters),
				Timestamp = DateTime.Now,
				IsPreview = isPreview,
			};
			db.Certificates.Add(certificate);
			await db.SaveChangesAsync();
			return certificate;
		}

		public async Task ChangeTemplateArchiveName(Guid templateId, string newArchiveName)
		{
			var template = await FindTemplateById(templateId);
			if (template == null)
				throw new ArgumentException("Invalid templateId", nameof(templateId));

			template.ArchiveName = newArchiveName;
			await db.SaveChangesAsync();
		}

		public async Task ChangeTemplateName(Guid templateId, string name)
		{
			var template = await FindTemplateById(templateId);
			if (template == null)
				throw new ArgumentException("Invalid templateId", nameof(templateId));

			template.Name = name;
			await db.SaveChangesAsync();
		}

		public async Task<Dictionary<Guid, List<Certificate>>> GetCertificates(string courseId, bool includePreviews = false)
		{
			var certificates = db.Certificates
				.Where(c => c.Template.CourseId == courseId && !c.IsDeleted);
			if (!includePreviews)
				certificates = certificates.Where(c => !c.IsPreview);
			return (await certificates
				.Include(c => c.User)
				.Include(c => c.Instructor)
				.ToListAsync())
				.GroupBy(c => c.TemplateId)
				.ToDictionary(g => g.Key, g => g.OrderBy(c => c.Timestamp).ToList());
		}

		public async Task RemoveTemplate(CertificateTemplate template)
		{
			template.IsDeleted = true;
			await db.SaveChangesAsync();
		}

		public async Task<List<Certificate>> GetUserCertificates(string userId, bool includePreviews = false)
		{
			var certificates = db.Certificates.Where(c => c.UserId == userId && !c.IsDeleted);
			if (!includePreviews)
				certificates = certificates.Where(c => !c.IsPreview);
			return await certificates.ToListAsync();
		}

		public async Task RemoveCertificate(Certificate certificate)
		{
			certificate.IsDeleted = true;
			await db.SaveChangesAsync();
		}

		public async Task AddCertificateTemplateArchive(string archiveName, Guid certificateTemplateId, byte[] content)
		{
			var archive = new CertificateTemplateArchive
			{
				ArchiveName = archiveName,
				CertificateTemplateId = certificateTemplateId,
				Content = content
			};
			db.CertificateTemplateArchives.Add(archive);
			await db.SaveChangesAsync();
		}
	}
}