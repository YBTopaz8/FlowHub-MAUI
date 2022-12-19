using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomSheet;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;
using LP = Android.Views.ViewGroup.LayoutParams;
using AColor = Android.Graphics.Color;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Orientation = Android.Widget.Orientation;


namespace FlowHub;
public class MyShellRenderer : ShellRenderer
{
    protected override IShellItemRenderer CreateShellItemRenderer(ShellItem shellItem)
    {
        return new MyShellItemRenderer(this);
    }
}

public class MyShellItemRenderer : ShellItemRenderer
{
    public MyShellItemRenderer(IShellContext context)
        : base(context)
    {
    }

    List<(string title, ImageSource icon, bool tabEnabled)> CreateTabList(ShellItem shellItem)
    {
        var items = new List<(string title, ImageSource icon, bool tabEnabled)>();
        var shellItems = ((IShellItemController)shellItem).GetItems();

        for (int i = 0; i < shellItems.Count; i++)
        {
            var item = shellItems[i];
            items.Add((item.Title, item.Icon, item.IsEnabled));
        }
        return items;
    }

    void OnMoreItemSelected(int shellSectionIndex, BottomSheetDialog dialog)
    {
        OnMoreItemSelected(ShellItemController.GetItems()[shellSectionIndex], dialog);
    }

    public static bool IsDisposed(Java.Lang.Object obj)
    {
        return obj.Handle == IntPtr.Zero;
    }

    protected override BottomSheetDialog CreateMoreBottomSheet(Action<int, BottomSheetDialog> selectCallback)
    {
        var bottomSheetDialog = new BottomSheetDialog(Context);
        var bottomSheetLayout = new LinearLayout(Context);
        using (var bottomShellLP = new LP(LP.MatchParent, LP.WrapContent))
            bottomSheetLayout.LayoutParameters = bottomShellLP;
        bottomSheetLayout.Orientation = Orientation.Vertical;

        // handle the more tab
        var items = ((IShellItemController)ShellItem).GetItems();
        // Here I changed _bottomView.MaxItemCount to hardcoded 5
        for (int i = 5 - 1; i < items.Count; i++)
        {
            var closure_i = i;
            var shellContent = items[i];

            using (var innerLayout = new LinearLayout(Context))
            {
                innerLayout.SetClipToOutline(true);
                innerLayout.SetBackground(CreateItemBackgroundDrawable());
                innerLayout.SetPadding(0, (int)Context.ToPixels(6), 0, (int)Context.ToPixels(6));
                innerLayout.Orientation = Orientation.Horizontal;
                using (var param = new LP(LP.MatchParent, LP.WrapContent))
                    innerLayout.LayoutParameters = param;

                // technically the unhook isn't needed
                // we dont even unhook the events that dont fire
                void clickCallback(object s, EventArgs e)
                {
                    selectCallback(closure_i, bottomSheetDialog);
                    if (!IsDisposed(innerLayout))
                        innerLayout.Click -= clickCallback;
                }

                innerLayout.Click += clickCallback;

                var image = new ImageView(Context);
                var lp = new LinearLayout.LayoutParams((int)Context.ToPixels(32), (int)Context.ToPixels(32))
                {
                    LeftMargin = (int)Context.ToPixels(20),
                    RightMargin = (int)Context.ToPixels(20),
                    TopMargin = (int)Context.ToPixels(6),
                    BottomMargin = (int)Context.ToPixels(6),
                    Gravity = GravityFlags.Center
                };
                image.LayoutParameters = lp;
                lp.Dispose();

                var services = ShellContext.Shell.Handler.MauiContext.Services;
                var provider = services.GetRequiredService<IImageSourceServiceProvider>();
                var icon = shellContent.Icon;

                shellContent.Icon.LoadImage(
                    ShellContext.Shell.Handler.MauiContext,
                    (result) =>
                            {
                                image.SetImageDrawable(result?.Value);
                                if (result?.Value != null)
                                {
                                    var color = Colors.Black.MultiplyAlpha(0.6f).ToPlatform();
                                    result.Value.SetTint(tintColor: color);
                                }
                            });

                innerLayout.AddView(image);

                using (var text = new TextView(Context))
                {
                    text.Typeface = services.GetRequiredService<IFontManager>()
                        .GetTypeface(Microsoft.Maui.Font.OfSize("sans-serif-medium", 0.0));

                    // CHANGE TEXT COLOR HERE
                    text.SetTextColor(AColor.AliceBlue);
                    text.Text = shellContent.Title;
                    lp = new LinearLayout.LayoutParams(0, LP.WrapContent)
                    {
                        Gravity = GravityFlags.Center,
                        Weight = 1
                    };
                    text.LayoutParameters = lp;
                    lp.Dispose();

                    innerLayout.AddView(text);
                }

                bottomSheetLayout.AddView(innerLayout);
            }
        }

        bottomSheetDialog.SetContentView(bottomSheetLayout);
        bottomSheetLayout.Dispose();

        return bottomSheetDialog;
    }

    protected override Drawable CreateItemBackgroundDrawable()
    {
        var stateList = ColorStateList.ValueOf(Colors.Black.MultiplyAlpha(0.2f).ToPlatform());

        // CHANGE BACKGROUND COLOR HERE
        var colorDrawable = new ColorDrawable(AColor.DarkSlateBlue);
        return new RippleDrawable(stateList, colorDrawable, null);
    }

    protected override bool OnItemSelected(IMenuItem item)
    {
        var id = item.ItemId;
        if (id == MoreTabId)
        {
            var items = CreateTabList(ShellItem);
            var _bottomSheetDialog = CreateMoreBottomSheet((int a, BottomSheetDialog b) => OnMoreItemSelected(a, b));

            _bottomSheetDialog.Show();
            _bottomSheetDialog.DismissEvent += OnMoreSheetDismissed;

            return true;
        }

        return base.OnItemSelected(item);
    }
}
