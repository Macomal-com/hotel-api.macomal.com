﻿using Repository.RequestDTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class BookingDetail : ICommonProperties
    {
        [Key]
        public int BookingId { get; set; }
        public int GuestId { get; set; }
        public int RoomId { get; set; }
        public int RoomTypeId { get; set; }
        
        public DateOnly CheckInDate { get; set; } 
        public string CheckInTime { get; set; } = string.Empty;
        public DateOnly CheckOutDate { get; set; }
        public string CheckOutTime { get; set; } = string.Empty;
        public DateTime CheckInDateTime { get; set; }
        public DateTime CheckOutDateTime { get; set; }
        public string CheckoutFormat { get; set; } = string.Empty;
        public int NoOfNights { get; set; }
        public int NoOfHours { get; set; }
        
        
        public int RoomCount { get; set; }
        public int Pax { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        
        public string ReservationNo { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        
        public int PrimaryGuestId { get; set; }
        public decimal InitialBalanceAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public decimal AdvanceAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public string AdvanceReceiptNo { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public DateOnly InvoiceDate { get; set; } = new DateOnly(1900, 01, 01);
        public string InvoiceNo { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string GstType { get; set; } = string.Empty;
        public int CompanyId { get; set; }

        public decimal BookingAmount { get; set; }
        public decimal GstAmount { get; set; }
        public decimal TotalBookingAmount { get; set; }
        public string BookingSource { get; set; } = string.Empty;

        public DateOnly ReservationDate { get; set; }
        public string ReservationTime { get; set; } = string.Empty;
        public DateTime ReservationDateTime { get; set; }

        public DateOnly InitialCheckOutDate { get; set; }
        public string InitialCheckOutTime { get; set; } = string.Empty;
        public DateTime InitialCheckOutDateTime { get; set; }
        public decimal ServicesAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public decimal AgentAdvanceAmount { get; set; }
        public string InvoiceName { get; set; } = string.Empty;
        public string BillTo { get; set; } = string.Empty;

        public decimal TotalServicesAmount { get; set; }
        public decimal ServicesTaxAmount { get; set; }

        public decimal CancelAmount { get; set; }
        public DateTime CancelDate { get; set; } = new DateTime(1900, 01, 01);
        public string CheckOutInvoiceFormat { get; set; } = string.Empty;
        public bool IsEarlyCheckIn { get; set; }
        public string EarlyCheckInPolicyName { get; set; } = string.Empty;

        public string EarlyCheckInDeductionBy { get; set; } = string.Empty;
        public string EarlyCheckInApplicableOn { get; set; }
        = string.Empty;

        public int EarlyCheckInFromHour { get; set; }
        public int EarlyCheckInToHour { get; set; }

        public decimal EarlyCheckInCharges { get; set; }

        public bool IsLateCheckOut { get; set; }
        public string LateCheckOutPolicyName { get; set; } = string.Empty;

        public string LateCheckOutDeductionBy { get; set; } = string.Empty;
        public string LateCheckOutApplicableOn { get; set; }
        = string.Empty;

        public int LateCheckOutFromHour { get; set; }
        public int LateCheckOutToHour { get; set; }

        public decimal LateCheckOutCharges { get; set; }

        public string DiscountType { get; set; } = string.Empty;

        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }

        public decimal DiscountTotalAmount { get; set; }
        public decimal BookingAmountWithoutDiscount { get; set; }

        public string CalculateRoomRates { get; set; } = string.Empty;
        public decimal TransactionCharges { get; set; } 
        public decimal AgentServiceCharge { get; set; }

        public decimal ResidualAmount { get; set; }

        public string CheckOutDiscountType { get; set; } = string.Empty;

        public decimal CheckOutDiscountPercentage { get; set; }

        public decimal CheckOutDiscoutAmount { get; set; }

        public decimal TotalAmountWithOutDiscount { get; set; }

        public decimal EachNightRate { get; set; }
        public decimal EachNightRateWithoutDiscount { get; set; }

        [NotMapped]
        public bool IsSelectedValue { get; set; }

        [NotMapped]
        public string RoomTypeName { get; set; } = string.Empty;

        [NotMapped]
        public string RoomNo { get; set; } = string.Empty;

        [NotMapped]
        public List<BookedRoomRate> BookedRoomRates { get; set; } = new List<BookedRoomRate>();

        [NotMapped]
        public List<RoomCancelHistory> RoomCancelHistory { get; set; } = new List<RoomCancelHistory>();

        [NotMapped]
        public GuestDetails GuestDetails = new GuestDetails();

        [NotMapped]
        public List<AdvanceService> AdvanceServices = new List<AdvanceService>();
    }

    public class BookingDetailDTO
    {
        public DateOnly CheckInDate { get; set; }
        public string CheckInTime { get; set; } = string.Empty;
        public DateOnly CheckOutDate { get; set; }
        public string CheckOutTime { get; set; } = string.Empty;
        public int RoomTypeId { get; set; }
        public string RoomCategoryName { get; set; } = string.Empty;
        public int NoOfNights { get; set; }
        public int NoOfHours { get; set; }
     
        public string GstType { get; set; } = string.Empty;

        public int NoOfRooms { get; set; }
        public int Pax { get; set; }
        public string Remarks { get; set; } = string.Empty;

        public List<RoomData> AssignedRooms { get; set; } = new List<RoomData>();
        public string Status { get; set; } = string.Empty;

        public string CheckOutFormat { get; set; } = string.Empty;

        public decimal BookingAmount { get; set; }
        public decimal GstAmount { get; set; }
        public decimal TotalBookingAmount { get; set; }
        public string BookingSource { get; set; } = string.Empty;
        public bool IsEarlyCheckIn { get; set; }
        public string EarlyCheckInPolicyName { get; set; } = string.Empty;

        public string EarlyCheckInDeductionBy { get; set; } = string.Empty;
        public string EarlyCheckInApplicableOn { get; set; }
        = string.Empty;

        public int EarlyCheckInFromHour { get; set; }
        public int EarlyCheckInToHour { get; set; }

        public decimal EarlyCheckInCharges { get; set; }

        public bool IsLateCheckOut { get; set; }
        public string LateCheckOutPolicyName { get; set; } = string.Empty;

        public string LateCheckOutDeductionBy { get; set; } = string.Empty;
        public string LateCheckOutApplicableOn { get; set; }
        = string.Empty;

        public int LateCheckOutFromHour { get; set; }
        public int LateCheckOutToHour { get; set; }

        public decimal LateCheckOutCharges { get; set; }

        public string DiscountType { get; set; } = string.Empty;


        public decimal DiscountTotalAmount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal BookingAmountWithoutDiscount { get; set; }
        public string CalculateRoomRates { get; set; } = string.Empty;
        public decimal EachNightRate { get; set; }
        public decimal EachNightRateWithoutDiscount { get; set; }

    }

    public class BookingDetailCheckInDTO
    {
        public int BookingId { get; set; }
        public int GuestId { get; set; }
        public int RoomId { get; set; }

        public string RoomNo { get; set; } = string.Empty;
        public int RoomTypeId { get; set; }
        public string RoomCategoryName { get; set; } = string.Empty;
        public string CheckInDate { get; set; } = string.Empty;
        public string CheckInTime { get; set; } = string.Empty;
        public string CheckOutDate { get; set; } = string.Empty;
        public string CheckOutTime { get; set; } = string.Empty;
        public DateTime CheckInDateTime { get; set; }
        public DateTime CheckOutDateTime { get; set; }
        public string CheckOutFormat { get; set; } = string.Empty;
        public int NoOfNights { get; set; }
        public int NoOfHours { get; set; }


        public string Status { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;

        public string ReservationNo { get; set; } = string.Empty;

        public int UserId { get; set; }

        public int CompanyId { get; set; }

        public decimal BookingAmount { get; set; }
        public string GstType { get; set; } = string.Empty;
        public decimal GstAmount { get; set; }
        public decimal TotalBookingAmount { get; set; }
        public string BookingSource { get; set; } = string.Empty;
        public string ReservationDate { get; set; } = string.Empty;
        public string ReservationTime { get; set; } = string.Empty;
        public DateTime ReservationDateTime { get; set; }
        public int Pax { get; set; }

        public List<BookedRoomRate> BookedRoomRates = new List<BookedRoomRate>();

        public GuestDetails GuestDetails = new GuestDetails();

        public bool IsSameGuest { get; set; }
        public DateTime OriginalReservationDateTime { get; set; } 
        public string OriginalReservationDate { get; set; } = string.Empty;
        public string OriginalReservationTime { get; set; } = string.Empty;

        public string OriginalCheckInDate { get; set; } = string.Empty;
        public string OriginalCheckInTime { get; set; } = string.Empty;

        public string OriginalCheckOutDate { get; set; } = string.Empty;
        public string OriginalCheckOutTime { get; set; } = string.Empty;
        public int OriginalRoomTypeId {get;set;}
        public string OriginalRoomCategoryName { get; set; } = string.Empty;
        public int OriginalRoomId { get; set; }
        public string OriginalRoomNo { get; set; } = string.Empty;
        public bool IsEarlyCheckIn { get; set; }
        public string EarlyCheckInPolicyName { get; set; } = string.Empty;

        public string EarlyCheckInDeductionBy { get; set; } = string.Empty;
        public string EarlyCheckInApplicableOn { get; set; }
        = string.Empty;

        public int EarlyCheckInFromHour { get; set; }
        public int EarlyCheckInToHour { get; set; }

        public decimal EarlyCheckInCharges { get; set; }
        public bool IsLateCheckOut { get; set; }
        public string LateCheckOutPolicyName { get; set; } = string.Empty;

        public string LateCheckOutDeductionBy { get; set; } = string.Empty;
        public string LateCheckOutApplicableOn { get; set; }
        = string.Empty;

        public int LateCheckOutFromHour { get; set; }
        public int LateCheckOutToHour { get; set; }

        public decimal LateCheckOutCharges { get; set; }
        public decimal TotalServicesAmount { get; set; }
        public int OriginalPax { get; set; }
        [NotMapped]
        public string GuestName { get; set; } = string.Empty;
        [NotMapped]
        public string GuestPhone { get; set; } = string.Empty;
        [NotMapped]
        public decimal TotalAmount { get; set; }
        [NotMapped]
        public decimal ServicesAmount { get; set; }

        public bool IsCheckIn { get; set; }

        [NotMapped]
        public bool IsCheckBox { get; set; }
    }


   
}


