using System;

namespace DoAnCSharp.Models;

public class PlayHistory
{
    // ID của bản ghi lịch sử
    public int Id { get; set; }

    // Liên kết với User (Ai là người đã nghe review này?)
    public int UserId { get; set; }

    // Liên kết với Quán ăn (Review của quán nào?)
    public int PoiId { get; set; }

    // Tên quán và Ảnh (Dùng để hiển thị nhanh trên giao diện Lịch sử)
    public string PoiName { get; set; } = string.Empty;
    public string ImageAsset { get; set; } = string.Empty;

    // Thời điểm nghe
    public DateTime PlayedAt { get; set; }

    // Hàm bổ trợ để hiển thị ngày tháng đẹp hơn trên UI
    public string PlayedAtString => PlayedAt.ToString("dd/MM/yyyy HH:mm");
}