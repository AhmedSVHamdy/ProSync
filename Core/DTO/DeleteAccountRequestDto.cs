using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTO
{
    public class DeleteAccountRequestDto
    {
        public string Password { get; set; } = string.Empty;   // تأكيد إضافي قبل الحذف النهائي
    }
}
