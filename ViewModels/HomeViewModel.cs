using CommunityToolkit.Mvvm.ComponentModel;

namespace DoAnCSharp.ViewModels;

// Phải kế thừa ObservableObject và dùng partial class theo chuẩn MVVM Toolkit
public partial class HomeViewModel : ObservableObject
{
    public HomeViewModel()
    {
        // Tạm thời để trống để App khởi động thành công trước.
        // Sau này chúng ta sẽ viết code gọi danh sách quán ăn từ PoiRepository ở đây.
    }
}
