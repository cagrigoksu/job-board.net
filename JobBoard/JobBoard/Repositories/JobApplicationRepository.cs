﻿using JobBoard.DataContext;
using JobBoard.Models.Data;
using JobBoard.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.IO;
using JobBoard.Enums;
using JobBoard.Models.Classes;
using Microsoft.Extensions.Hosting;

namespace JobBoard.Repositories
{
    public class JobApplicationRepository : IJobApplicationRepository
    {
        private readonly AppDbContext _db;
        public JobApplicationRepository(AppDbContext db)
        {
            _db = db;
        }


        public JobApplicationDataModel GetUserJobApplication(int userId, int jobId)
        {
            var application = _db.JobApplications.SingleOrDefault(x => x.ApplicantId == userId 
                                                                       && x.JobId == jobId
                                                                       && x.IsDeleted == false);
            return application;
        }

        public void AddJobApplication(JobApplicationDataModel jobApplication)
        {
            jobApplication.ApplicationDate = DateTime.Now;
                
            _db.Add(jobApplication);
            _db.SaveChanges();
        }

        public IQueryable<AppliedJobsListModel> GetUserBasedJobApplications(int userId)
        {
            var jobList =
                from post in _db.JobPosts
                join application in _db.JobApplications on post.Id equals application.JobId
                where application.ApplicantId == userId
                      && !application.IsDeleted
                      && !post.IsDeleted
                select new AppliedJobsListModel()
                {
                    Title = post.Title,
                    ApplicationDate = application.ApplicationDate,
                    City = post.City,
                    CompanyId = post.CompanyId,
                    Id = application.Id,
                    JobId = application.JobId,
                    Status = application.Status
                };

            return jobList;
        }

        public void WithdrawJobApplication(int jobId)
        {
            var dbPost = _db.JobApplications.FirstOrDefault(x => x.JobId == jobId && x.IsDeleted == false);

            dbPost.DeleteDate = DateTime.Now;
            dbPost.IsDeleted = true;
            dbPost.DeleteUser = Globals.UserId;

            _db.Update(dbPost);
            _db.SaveChanges();
        }

        public IQueryable<JobApplicantsListModel> GetJobApplicantsList(int jobId)
        {
            var applicants = from job in _db.JobPosts
                join app in _db.JobApplications on job.Id equals app.JobId
                join user in _db.UserProfiles on app.ApplicantId equals user.UserId
                where job.Id == jobId && job.IsDeleted == false && app.IsDeleted == false
                select new JobApplicantsListModel()
                {
                    ApplicantId = app.ApplicantId,
                    Name = user.Name ,
                    Surname = user.Surname ,
                    ApplicationDate = app.ApplicationDate,
                    ResumeURL = app.UrlResume,
                    MotivationLetterURL = app.UrlMotivationLetter,
                    Status = app.Status
                };

            return applicants;
        }
    }
}