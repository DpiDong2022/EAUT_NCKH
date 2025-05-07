using System;
using System.Collections.Generic;
using EAUT_NCKH.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EAUT_NCKH.Web.Data;

public partial class EntityDataContext : DbContext
{
    public EntityDataContext()
    {
    }

    public EntityDataContext(DbContextOptions<EntityDataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Academictitle> Academictitles { get; set; }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Building> Buildings { get; set; }

    public virtual DbSet<Committee> Committees { get; set; }

    public virtual DbSet<Committeemember> Committeemembers { get; set; }

    public virtual DbSet<Committeerole> Committeeroles { get; set; }

    public virtual DbSet<Committeetype> Committeetypes { get; set; }

    public virtual DbSet<Criteriaevaluationtype> Criteriaevaluationtypes { get; set; }

    public virtual DbSet<Defenseassignment> Defenseassignments { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Evaluationimage> Evaluationimages { get; set; }

    public virtual DbSet<Finalresult> Finalresults { get; set; }

    public virtual DbSet<Finalresultevaluation> Finalresultevaluations { get; set; }

    public virtual DbSet<Major> Majors { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Notificationaccount> Notificationaccounts { get; set; }

    public virtual DbSet<Proposal> Proposals { get; set; }

    public virtual DbSet<Proposalevaluation> Proposalevaluations { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Substatus> Substatuses { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<Topic> Topics { get; set; }

    public virtual DbSet<Topicstatus> Topicstatuses { get; set; }

    public virtual DbSet<Topicstudent> Topicstudents { get; set; }

    public virtual DbSet<Trainingprogram> Trainingprograms { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.\\DONGSQLSERVER;Database=EautNckh;User Id=sa;Password=123456; TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Academictitle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__academic__3213E83FEE947661");

            entity.ToTable("academictitle");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.Displayorder).HasColumnName("displayorder");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__account__3213E83FE6132861");

            entity.ToTable("account");

            entity.HasIndex(e => e.Phonenumber, "UQ__account__622BF0C257A830F2").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__account__AB6E61641D50335D").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Avatar)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("avatar");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createddate");
            entity.Property(e => e.Departmentid).HasColumnName("departmentid");
            entity.Property(e => e.Email)
                .HasMaxLength(120)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(100)
                .HasColumnName("fullname");
            entity.Property(e => e.Otp)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("otp");
            entity.Property(e => e.Otpgeneratedat)
                .HasColumnType("datetime")
                .HasColumnName("otpgeneratedat");
            entity.Property(e => e.Password)
                .HasMaxLength(120)
                .HasDefaultValue("Abc@123")
                .HasColumnName("password");
            entity.Property(e => e.Phonenumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phonenumber");
            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Sex).HasColumnName("sex");
            entity.Property(e => e.Updateddate)
                .HasColumnType("datetime")
                .HasColumnName("updateddate");

            entity.HasOne(d => d.Department).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.Departmentid)
                .HasConstraintName("FK_AccounDepartment");

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.Roleid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AccounRole");
        });

        modelBuilder.Entity<Building>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__building__3213E83F3EA64D98");

            entity.ToTable("building");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Displayorder).HasColumnName("displayorder");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Committee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__committe__3213E83F0919908F");

            entity.ToTable("committee");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Buildingid).HasColumnName("buildingid");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createddate");
            entity.Property(e => e.Eventdate)
                .HasColumnType("datetime")
                .HasColumnName("eventdate");
            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .HasColumnName("name");
            entity.Property(e => e.Roomid).HasColumnName("roomid");
            entity.Property(e => e.Topicid).HasColumnName("topicid");
            entity.Property(e => e.Typeid).HasColumnName("typeid");
            entity.Property(e => e.Updateddate)
                .HasColumnType("datetime")
                .HasColumnName("updateddate");

            entity.HasOne(d => d.Building).WithMany(p => p.Committees)
                .HasForeignKey(d => d.Buildingid)
                .HasConstraintName("FK_Committee_Building");

            entity.HasOne(d => d.Room).WithMany(p => p.Committees)
                .HasForeignKey(d => d.Roomid)
                .HasConstraintName("FK_Committee_Room");

            entity.HasOne(d => d.Topic).WithMany(p => p.Committees)
                .HasForeignKey(d => d.Topicid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Committee_Topic");

            entity.HasOne(d => d.Type).WithMany(p => p.Committees)
                .HasForeignKey(d => d.Typeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Committee_Type");
        });

        modelBuilder.Entity<Committeemember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__committe__3213E83FD594ECB5");

            entity.ToTable("committeemember");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Accountid).HasColumnName("accountid");
            entity.Property(e => e.Committeeid).HasColumnName("committeeid");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createddate");
            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Updateddate)
                .HasColumnType("datetime")
                .HasColumnName("updateddate");

            entity.HasOne(d => d.Account).WithMany(p => p.Committeemembers)
                .HasForeignKey(d => d.Accountid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CommitteeMember_Account2");

            entity.HasOne(d => d.Committee).WithMany(p => p.Committeemembers)
                .HasForeignKey(d => d.Committeeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CommitteeMember_Committee2");

            entity.HasOne(d => d.Role).WithMany(p => p.Committeemembers)
                .HasForeignKey(d => d.Roleid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CommitteeMember_Role2");
        });

        modelBuilder.Entity<Committeerole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__committe__3213E83FE78E04AC");

            entity.ToTable("committeerole");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Committeetypeid).HasColumnName("committeetypeid");
            entity.Property(e => e.Displayorder).HasColumnName("displayorder");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");

            entity.HasOne(d => d.Committeetype).WithMany(p => p.Committeeroles)
                .HasForeignKey(d => d.Committeetypeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Committeerole_committeetype");
        });

        modelBuilder.Entity<Committeetype>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__committe__3213E83FA0AD4F73");

            entity.ToTable("committeetype");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Displayorder).HasColumnName("displayorder");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Criteriaevaluationtype>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__criteria__3213E83F3A659AD6");

            entity.ToTable("criteriaevaluationtype");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content)
                .HasMaxLength(1000)
                .HasColumnName("content");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createddate");
            entity.Property(e => e.Maxscore).HasColumnName("maxscore");
            entity.Property(e => e.Type).HasColumnName("type");
        });

        modelBuilder.Entity<Defenseassignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__defensea__3213E83F0F345E33");

            entity.ToTable("defenseassignment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createddate");
            entity.Property(e => e.Finalscore).HasColumnName("finalscore");
            entity.Property(e => e.Topicid).HasColumnName("topicid");
            entity.Property(e => e.Updateddate)
                .HasColumnType("datetime")
                .HasColumnName("updateddate");

            entity.HasOne(d => d.Topic).WithMany(p => p.Defenseassignments)
                .HasForeignKey(d => d.Topicid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_defenseassignmentopic");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__departme__3213E83F1F22CDC8");

            entity.ToTable("department");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Displayorder).HasColumnName("displayorder");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Evaluationimage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__evaluati__3213E83F99303EC4");

            entity.ToTable("evaluationimage");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createddate");
            entity.Property(e => e.Defenseassignmentid).HasColumnName("defenseassignmentid");
            entity.Property(e => e.Filename)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("filename");
            entity.Property(e => e.Filepath)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("filepath");
            entity.Property(e => e.Updateddate)
                .HasColumnType("datetime")
                .HasColumnName("updateddate");

            entity.HasOne(d => d.Defenseassignment).WithMany(p => p.Evaluationimages)
                .HasForeignKey(d => d.Defenseassignmentid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_evaluationimage_defenseassignment");
        });

        modelBuilder.Entity<Finalresult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__finalres__3213E83F6BD1BFDB");

            entity.ToTable("finalresult");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createddate");
            entity.Property(e => e.Filepath)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("filepath");
            entity.Property(e => e.Submitdate)
                .HasColumnType("datetime")
                .HasColumnName("submitdate");
            entity.Property(e => e.Submitdeadline)
                .HasColumnType("datetime")
                .HasColumnName("submitdeadline");
            entity.Property(e => e.Topicid).HasColumnName("topicid");
            entity.Property(e => e.Updateddate)
                .HasColumnType("datetime")
                .HasColumnName("updateddate");

            entity.HasOne(d => d.Topic).WithMany(p => p.Finalresults)
                .HasForeignKey(d => d.Topicid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_finalresultopic");
        });

        modelBuilder.Entity<Finalresultevaluation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__finalres__3213E83FC3CB9244");

            entity.ToTable("finalresultevaluation");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdby).HasColumnName("createdby");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createddate");
            entity.Property(e => e.Criteriaid).HasColumnName("criteriaid");
            entity.Property(e => e.Finalresultid).HasColumnName("finalresultid");
            entity.Property(e => e.Topicid).HasColumnName("topicid");
            entity.Property(e => e.Updateddate)
                .HasColumnType("datetime")
                .HasColumnName("updateddate");
            entity.Property(e => e.Value)
                .HasMaxLength(255)
                .HasColumnName("value");

            entity.HasOne(d => d.CreatedbyNavigation).WithMany(p => p.Finalresultevaluations)
                .HasForeignKey(d => d.Createdby)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_finalresultevaluation_createdby");

            entity.HasOne(d => d.Criteria).WithMany(p => p.Finalresultevaluations)
                .HasForeignKey(d => d.Criteriaid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_finalresultevaluation_criteria");

            entity.HasOne(d => d.Finalresult).WithMany(p => p.Finalresultevaluations)
                .HasForeignKey(d => d.Finalresultid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_finalresultevaluation_finalresult");

            entity.HasOne(d => d.Topic).WithMany(p => p.Finalresultevaluations)
                .HasForeignKey(d => d.Topicid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_finalresultevaluation_topic");
        });

        modelBuilder.Entity<Major>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__major__3213E83FA957E948");

            entity.ToTable("major");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.Departmentid).HasColumnName("departmentid");
            entity.Property(e => e.Displayorder).HasColumnName("displayorder");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.HasOne(d => d.Department).WithMany(p => p.Majors)
                .HasForeignKey(d => d.Departmentid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MajorDepartment");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__notifica__3213E83FB3E20E19");

            entity.ToTable("notification");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createddate");
            entity.Property(e => e.Notificationtemplate)
                .HasMaxLength(2000)
                .HasColumnName("notificationtemplate");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.Updateddate)
                .HasColumnType("datetime")
                .HasColumnName("updateddate");
        });

        modelBuilder.Entity<Notificationaccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__notifica__3213E83FE4B325BC");

            entity.ToTable("notificationaccount");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createddate");
            entity.Property(e => e.Isread).HasColumnName("isread");
            entity.Property(e => e.Link)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("link");
            entity.Property(e => e.Notificationcontent)
                .HasMaxLength(1000)
                .HasColumnName("notificationcontent");
            entity.Property(e => e.Notificationid).HasColumnName("notificationid");
            entity.Property(e => e.Receiverid).HasColumnName("receiverid");

            entity.HasOne(d => d.Notification).WithMany(p => p.Notificationaccounts)
                .HasForeignKey(d => d.Notificationid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_notificationaccounnotification");

            entity.HasOne(d => d.Receiver).WithMany(p => p.Notificationaccounts)
                .HasForeignKey(d => d.Receiverid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_notificationaccounaccount");
        });

        modelBuilder.Entity<Proposal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__proposal__3213E83F66F56763");

            entity.ToTable("proposal");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createddate");
            entity.Property(e => e.Filepath)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("filepath");
            entity.Property(e => e.Note)
                .HasMaxLength(250)
                .HasColumnName("note");
            entity.Property(e => e.Submitdate)
                .HasColumnType("datetime")
                .HasColumnName("submitdate");
            entity.Property(e => e.Submitdeadline)
                .HasColumnType("datetime")
                .HasColumnName("submitdeadline");
            entity.Property(e => e.Topicid).HasColumnName("topicid");
            entity.Property(e => e.Updateddate)
                .HasColumnType("datetime")
                .HasColumnName("updateddate");

            entity.HasOne(d => d.Topic).WithMany(p => p.Proposals)
                .HasForeignKey(d => d.Topicid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Proposal_Topic");
        });

        modelBuilder.Entity<Proposalevaluation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__proposal__3213E83FC46651E7");

            entity.ToTable("proposalevaluation");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Approverid).HasColumnName("approverid");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createddate");
            entity.Property(e => e.Feedback)
                .HasMaxLength(2000)
                .HasColumnName("feedback");
            entity.Property(e => e.Proposalid).HasColumnName("proposalid");
            entity.Property(e => e.Statusid).HasColumnName("statusid");
            entity.Property(e => e.Topicid).HasColumnName("topicid");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Updateddate)
                .HasColumnType("datetime")
                .HasColumnName("updateddate");

            entity.HasOne(d => d.Approver).WithMany(p => p.Proposalevaluations)
                .HasForeignKey(d => d.Approverid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProposalEvaluation_Approver");

            entity.HasOne(d => d.Proposal).WithMany(p => p.Proposalevaluations)
                .HasForeignKey(d => d.Proposalid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProposalEvaluation_Proposal");

            entity.HasOne(d => d.Status).WithMany(p => p.Proposalevaluations)
                .HasForeignKey(d => d.Statusid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_proposalevaluation_Substatus");

            entity.HasOne(d => d.Topic).WithMany(p => p.Proposalevaluations)
                .HasForeignKey(d => d.Topicid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProposalEvaluation_Topic");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__role__3213E83FACF09DCD");

            entity.ToTable("role");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Displayorder).HasColumnName("displayorder");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__room__3213E83FC53DAF18");

            entity.ToTable("room");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Buildingid).HasColumnName("buildingid");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");

            entity.HasOne(d => d.Building).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.Buildingid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Room_Building");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_student_id");

            entity.ToTable("student");

            entity.HasIndex(e => e.Phonenumber, "UQ__student__622BF0C29F5C6D73").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__student__AB6E616466E42BE5").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("id");
            entity.Property(e => e.Accountid).HasColumnName("accountid");
            entity.Property(e => e.Classname)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("classname");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createddate");
            entity.Property(e => e.Departmentid).HasColumnName("departmentid");
            entity.Property(e => e.Email)
                .HasMaxLength(120)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(100)
                .HasColumnName("fullname");
            entity.Property(e => e.Majorid).HasColumnName("majorid");
            entity.Property(e => e.Phonenumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phonenumber");
            entity.Property(e => e.Trainingprogramid).HasColumnName("trainingprogramid");
            entity.Property(e => e.Updateddate)
                .HasColumnType("datetime")
                .HasColumnName("updateddate");

            entity.HasOne(d => d.Account).WithMany(p => p.Students)
                .HasForeignKey(d => d.Accountid)
                .HasConstraintName("FK_StudenAccount");

            entity.HasOne(d => d.Department).WithMany(p => p.Students)
                .HasForeignKey(d => d.Departmentid)
                .HasConstraintName("FK_StudenDepartment");

            entity.HasOne(d => d.Major).WithMany(p => p.Students)
                .HasForeignKey(d => d.Majorid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Studenmajor");

            entity.HasOne(d => d.Trainingprogram).WithMany(p => p.Students)
                .HasForeignKey(d => d.Trainingprogramid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudenTrainingprogram");
        });

        modelBuilder.Entity<Substatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__substatu__3213E83F84F7C4C9");

            entity.ToTable("substatus");

            entity.HasIndex(e => e.Code, "UQ_substatus_code").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.Displayorder).HasColumnName("displayorder");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__teacher__3213E83FBD3C12B8");

            entity.ToTable("teacher");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Academictitleid)
                .HasDefaultValue(1)
                .HasColumnName("academictitleid");
            entity.Property(e => e.Accountid).HasColumnName("accountid");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createddate");
            entity.Property(e => e.Majorid).HasColumnName("majorid");
            entity.Property(e => e.Updateddate)
                .HasColumnType("datetime")
                .HasColumnName("updateddate");

            entity.HasOne(d => d.Academictitle).WithMany(p => p.Teachers)
                .HasForeignKey(d => d.Academictitleid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Teacher_academictitle");

            entity.HasOne(d => d.Account).WithMany(p => p.Teachers)
                .HasForeignKey(d => d.Accountid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Teacher_Account");

            entity.HasOne(d => d.Major).WithMany(p => p.Teachers)
                .HasForeignKey(d => d.Majorid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Teacher_major");
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__topic__3213E83F8676E274");

            entity.ToTable("topic");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdby).HasColumnName("createdby");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createddate");
            entity.Property(e => e.Departmentid).HasColumnName("departmentid");
            entity.Property(e => e.Enddate)
                .HasColumnType("datetime")
                .HasColumnName("enddate");
            entity.Property(e => e.Note)
                .HasMaxLength(600)
                .HasColumnName("note");
            entity.Property(e => e.Secondteacherid).HasColumnName("secondteacherid");
            entity.Property(e => e.Startdate)
                .HasColumnType("datetime")
                .HasColumnName("startdate");
            entity.Property(e => e.Status)
                .HasDefaultValue(3)
                .HasColumnName("status");
            entity.Property(e => e.Studentid).HasColumnName("studentid");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.Updateddate)
                .HasColumnType("datetime")
                .HasColumnName("updateddate");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.CreatedbyNavigation).WithMany(p => p.TopicCreatedbyNavigations)
                .HasForeignKey(d => d.Createdby)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Topic_CreatedBy");

            entity.HasOne(d => d.Department).WithMany(p => p.Topics)
                .HasForeignKey(d => d.Departmentid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Topic_Department");

            entity.HasOne(d => d.Secondteacher).WithMany(p => p.TopicSecondteachers)
                .HasForeignKey(d => d.Secondteacherid)
                .HasConstraintName("FK_Topic_SecondTeacher");

            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.Topics)
                .HasForeignKey(d => d.Status)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Topic_Status");

            entity.HasOne(d => d.Student).WithMany(p => p.TopicStudents)
                .HasForeignKey(d => d.Studentid)
                .HasConstraintName("FK_Topic_Student");
        });

        modelBuilder.Entity<Topicstatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__topicsta__3213E83FFEF2325D");

            entity.ToTable("topicstatus");

            entity.HasIndex(e => e.Code, "UQ_topicstatus_code").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.Displayorder).HasColumnName("displayorder");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Topicstudent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__topicstu__3213E83F1A5136FC");

            entity.ToTable("topicstudent");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createddate");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.Studentcode)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("studentcode");
            entity.Property(e => e.Topicid).HasColumnName("topicid");

            entity.HasOne(d => d.StudentcodeNavigation).WithMany(p => p.Topicstudents)
                .HasForeignKey(d => d.Studentcode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TopicStudenStudent");

            entity.HasOne(d => d.Topic).WithMany(p => p.Topicstudents)
                .HasForeignKey(d => d.Topicid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TopicStudenTopic");
        });

        modelBuilder.Entity<Trainingprogram>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__training__3213E83F6ADA4ADE");

            entity.ToTable("trainingprogram");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.Displayorder).HasColumnName("displayorder");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
