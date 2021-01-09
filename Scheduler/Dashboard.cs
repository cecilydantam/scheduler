﻿using Scheduler.Enum;
using Scheduler.Models;
using Scheduler.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Scheduler
{
    public partial class Dashboard : Form
    {
        public CustomerRepository CustomerRepo { get; set; }
        public AppointmentRepository AppointmentRepo { get; set; }
        public AddressRepository AddressRepo { get; set; }
        public User LoggedInUser { get; set; }
        public List<Appointment> Appointments { get; set; }
        public List<Customer> Customers { get; set; }

        public Dashboard(User user)
        {
            AddressRepo = new AddressRepository(user);
            CustomerRepo = new CustomerRepository(user, AddressRepo);
            AppointmentRepo = new AppointmentRepository(user);
            
            InitializeComponent();
            BindGrids();
        }

        public void BindGrids()
        {
            Customers = CustomerRepo.GetAllCustomers();
            dgCustomers.DataSource = Customers;
            // It is not a listed requirement to display address in data grid. Hiding address id column.
            dgCustomers.Columns["Address"].Visible = false;
            dgCustomers.Columns["AddressId"].Visible = false;
            Appointments = AppointmentRepo.GetAppointmentsForUser();
            dgAppointments.DataSource = Appointments;
        }

        private void btnAddCustomer_Click(object sender, EventArgs e)
        {
            var addCustomerForm = new CustomerDetail(DetailMode.Add, this, AddressRepo, CustomerRepo);
            addCustomerForm.Show();
        }

        private void btnModifyCustomer_Click(object sender, EventArgs e)
        {
            var selectedCustomer = (Customer)dgCustomers.SelectedRows[0].DataBoundItem;
            var modifyCustomerForm = new CustomerDetail(DetailMode.Modify, this, AddressRepo, CustomerRepo, selectedCustomer);
            modifyCustomerForm.Show();
        }

        private void btnDeleteCustomer_Click(object sender, EventArgs e)
        {
            var selectedCustomer = (Customer)dgCustomers.SelectedRows[0].DataBoundItem;
            AddressRepo.DeleteAddress(selectedCustomer.AddressId);
            CustomerRepo.DeleteCustomer(selectedCustomer.Id);
            BindGrids();
        }

        private void btnAddAppointment_Click(object sender, EventArgs e)
        {
            var addAppointmentForm = new AppointmentDetail(DetailMode.Add, this, AppointmentRepo, CustomerRepo, AddressRepo);
            addAppointmentForm.Show();
        }

        private void btnModifyAppointment_Click(object sender, EventArgs e)
        {
            var selectedAppointment = (Appointment)dgAppointments.SelectedRows[0].DataBoundItem;
            var modifyAppointmentForm = new AppointmentDetail(DetailMode.Modify, this, AppointmentRepo, CustomerRepo, AddressRepo, selectedAppointment);
            modifyAppointmentForm.Show();
        }

        private void btnDeleteAppointment_Click(object sender, EventArgs e)
        {
            var selectedAppointment = (Appointment)dgAppointments.SelectedRows[0].DataBoundItem;
            AppointmentRepo.DeleteAppointment(selectedAppointment.Id);
            BindGrids();
        }

        private void Dashboard_Click(Object sender, EventArgs e)
        {
            MessageBox.Show("You are in the Control.GotFocus event.");
        }

        private void reminder_Tick(object sender, EventArgs e)
        {
            var upcomingAppointment = CheckForUpcomingAppointment();
            MessageBox.Show("Alert! Upcoming Appointment");
        }

        private bool CheckForUpcomingAppointment()
        {
            var beginReminderInterval = DateTime.Now;
            var endReminderInterval = DateTime.Now.AddMinutes(15);

            foreach (var appointment in Appointments)
            {
                if (appointment.Start > beginReminderInterval && appointment.Start < endReminderInterval)
                {
                    return true;
                }
            }
            return false;
        }

        

        private void btnMasterSchedule_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Appointments are not currently tied to users in unalterable database schema.  Master Schedule available in next update.");
        }

        private void btnCustomersByCity_Click(object sender, EventArgs e)
        {
            var cities = AddressRepo.GetAllCities();
            var report = "";

            foreach (var city in cities)
            {
                var customersInThisCity = Customers.Where(c => c.Address.CityId == city.Id).ToList();
                report += $"{city.Name}: {customersInThisCity.Count} customers \n";
            }

            MessageBox.Show(report);
        }

        private void btnWeekly_Click(object sender, EventArgs e)
        {
            var weekly = new Weekly();
            weekly.Show();
        }

        private void btnTypesReport_Click(object sender, EventArgs e)
        {
            var appointments = AppointmentRepo.GetAllAppointments();

            var report = GenerateMonthlyTypesReport(appointments);
            MessageBox.Show(report);
        }

        private string GenerateMonthlyTypesReport(IList<Appointment> appointments)
        {
            var report = "";
            var januaryTypes = GetTypesOfAppointmentsForMonth(1, appointments);
            var februaryTypes = GetTypesOfAppointmentsForMonth(2, appointments);
            var marchTypes = GetTypesOfAppointmentsForMonth(3, appointments);
            var aprilTypes = GetTypesOfAppointmentsForMonth(4, appointments);
            var mayTypes = GetTypesOfAppointmentsForMonth(5, appointments);
            var juneTypes = GetTypesOfAppointmentsForMonth(6, appointments);
            var julyTypes = GetTypesOfAppointmentsForMonth(7, appointments);
            var augustTypes = GetTypesOfAppointmentsForMonth(8, appointments);
            var septemberTypes = GetTypesOfAppointmentsForMonth(9, appointments);
            var octoberTypes = GetTypesOfAppointmentsForMonth(10, appointments);
            var novemberTypes = GetTypesOfAppointmentsForMonth(11, appointments);
            var decemberTypes = GetTypesOfAppointmentsForMonth(12, appointments);

            report += ConstructReportForMonth("January", januaryTypes);
            report += ConstructReportForMonth("February", februaryTypes);
            report += ConstructReportForMonth("March", marchTypes);
            report += ConstructReportForMonth("April", aprilTypes);
            report += ConstructReportForMonth("May", mayTypes);
            report += ConstructReportForMonth("June", juneTypes);
            report += ConstructReportForMonth("July", julyTypes);
            report += ConstructReportForMonth("August", augustTypes);
            report += ConstructReportForMonth("September", septemberTypes);
            report += ConstructReportForMonth("October", octoberTypes);
            report += ConstructReportForMonth("November", novemberTypes);
            report += ConstructReportForMonth("December", decemberTypes);

            return report;
        }

        private string ConstructReportForMonth(string monthName, IList<string> types)
        {
            var results = "\n";

            if (types.Any())
            {
                results += $"{monthName} \n-------------";
                results += GetAggregatesForMonth(types);
            }
            else
            {
                results += $"No appointments for {monthName} \n";
            }

            return results;
        }

        private List<string> GetTypesOfAppointmentsForMonth(int monthValue, IList<Appointment> appointments)
        {
            var monthTypes = new List<string>();

            foreach (var appointment in appointments)
            {
                if (appointment.Start.Month == monthValue)
                    monthTypes.Add(appointment.Type);
            }

            return monthTypes;
        }

        private string GetAggregatesForMonth(IList<string> typesInAMonth)
        {
            var result = "\n";
            var q = from x in typesInAMonth
                    group x by x into g
                    let count = g.Count()
                    orderby count descending
                    select new { Type = g.Key, Count = count };
            foreach (var x in q)
            {
                result += $"Type: {x.Type} \n Count: {x.Count}  \n";
            }

            return result;
        }
    }
}
