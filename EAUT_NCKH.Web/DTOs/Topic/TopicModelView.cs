using EAUT_NCKH.Web.Models;

namespace EAUT_NCKH.Web.DTOs {
    public class TopicModelView : Topic {
        public string EncyptedID { get; set; }
        public bool Editabled { get; set; }

        public TopicModelView() {
        }

        public TopicModelView(Topic topic, string encryptedId = "", bool editabled = false) {
            // Map properties from the base Topic class
            this.Id = topic.Id;
            this.Title = topic.Title;
            this.Departmentid = topic.Departmentid;
            this.Studentid = topic.Studentid;
            this.Createdby = topic.Createdby;
            this.Secondteacherid = topic.Secondteacherid;
            this.Year = topic.Year;
            this.Startdate = topic.Startdate;
            this.Enddate = topic.Enddate;
            this.Createddate = topic.Createddate;
            this.Updateddate = topic.Updateddate;
            this.Status = topic.Status;
            this.Note = topic.Note;

            // Map navigation properties
            this.CreatedbyNavigation = topic.CreatedbyNavigation;
            this.Secondteacher = topic.Secondteacher;
            this.Student = topic.Student;
            this.Department = topic.Department;
            this.StatusNavigation = topic.StatusNavigation;
            this.Defenseassignments = topic.Defenseassignments;
            this.Finalresults = topic.Finalresults;
            this.Proposals = topic.Proposals;
            this.Committees = topic.Committees;
            this.Topicstudents = topic.Topicstudents;
            this.Proposalevaluations = topic.Proposalevaluations;
            this.Finalresults = topic.Finalresults;
            this.Finalresultevaluations = topic.Finalresultevaluations;

            // Set the new properties
            this.EncyptedID = encryptedId;
            this.Editabled = editabled;
        }
    }

    public class TopicDefenseModelView: Topic {
        public string EncyptedID { get; set; }
        public int TongDiem { get; set; }

        public TopicDefenseModelView() {
        }

        public TopicDefenseModelView(Topic topic, string encryptedId = "") {
            // Map properties from the base Topic class
            this.Id = topic.Id;
            this.Title = topic.Title;
            this.Departmentid = topic.Departmentid;
            this.Studentid = topic.Studentid;
            this.Createdby = topic.Createdby;
            this.Secondteacherid = topic.Secondteacherid;
            this.Year = topic.Year;
            this.Startdate = topic.Startdate;
            this.Enddate = topic.Enddate;
            this.Createddate = topic.Createddate;
            this.Updateddate = topic.Updateddate;
            this.Status = topic.Status;
            this.Note = topic.Note;

            // Map navigation properties
            this.CreatedbyNavigation = topic.CreatedbyNavigation;
            this.Secondteacher = topic.Secondteacher;
            this.Student = topic.Student;
            this.Department = topic.Department;
            this.StatusNavigation = topic.StatusNavigation;
            this.Defenseassignments = topic.Defenseassignments;
            this.Finalresults = topic.Finalresults;
            this.Proposals = topic.Proposals;
            this.Committees = topic.Committees;
            this.Topicstudents = topic.Topicstudents;
            this.Proposalevaluations = topic.Proposalevaluations;
            this.Finalresults = topic.Finalresults;
            this.Finalresultevaluations = topic.Finalresultevaluations;

            // Set the new properties
            this.EncyptedID = encryptedId;
        }
    }
}
