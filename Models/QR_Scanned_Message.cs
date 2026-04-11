using CommunityToolkit.Mvvm.Messaging.Messages;

namespace DoAnCSharp.Models
{
    // Lớp tin nhắn dùng để truyền nội dung QR quét được giữa các Page
    public class QrScannedMessage : ValueChangedMessage<string>
    {
        public QrScannedMessage(string value) : base(value)
        {
        }
    }
}