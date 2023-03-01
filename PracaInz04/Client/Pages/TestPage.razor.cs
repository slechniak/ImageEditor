using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using PracaInz04.Client.Controls;
using SkiaSharp;
using SkiaSharp.Views.Blazor;
using static PracaInz04.Client.IndexedDbClasses.IndexedDBModels;
using System.Diagnostics;
using PracaInz04.Client.Services;
using PracaInz04.Client.LocalStorageClasses;
using PracaInz04.Client.IndexedDbClasses;
using PracaInz04.Client.ImageProcessingClasses;

namespace PracaInz04.Client.Pages
{
    public partial class TestPage : IDisposable
    {
        [Inject]
        StateService SService { get; set; }
        [Inject]
        LocalStorageManager LSManager { get; set; }
        [Inject]
        IndexedDbManager IDbManager { get; set; }
        [Inject]
        IndexedDbContext IndexedDbContext { get; set; }
        [Inject]
        NavigationManager NavigationManager { get; set; }
        [Inject]
        ImageProcessing ImageProc { get; set; }
        [Inject]
        IJSRuntime JS { get; set; }

        [CascadingParameter] public IModalService Modal { get; set; }
        public IModalReference progressModal { get; set; }
        public string? ImageName { get; set; }
        public int? ImageId { get; set; }
        public ImageOriginal2 imageOriginal2 { get; set; }
        public ImageResized2 imageResized2 { get; set; }
        public SKBitmap sKBitmap { get; set; }

        public string testSrc { get; set; }
        public string testSrc2 { get; set; }
        SKCanvasView skiaView = null!;
        float scrollScale = 1;
        bool isDown = false;
        SKRect selectRect = SKRect.Empty;
        SKRect selectRectOriginal = SKRect.Empty;
        System.Timers.Timer clickTime = new System.Timers.Timer(100);
        bool isClick = true;
        SKRect bitmapRect = SKRect.Empty;
        float windowDPR = 1;
        SKPoint selectOffset;
        SKPoint selectOffsetOriginal;
        bool scrollScaleChanged = true;
        float distance;
        int selectCornerRadius = 12;
        private DotNetObjectReference<TestPage>? dotNetHelper;
        bool isMiddle = false;
        SKRect panRect = SKRect.Empty;
        SKPoint panOffset;
        List<SKBitmap> lastBitmaps = new List<SKBitmap>();
        int currentIndex = -1;
        int imax = 10;
        bool rotateSelect = false;
        float tiltAngle = 0;
        bool tilting = false;
        SKPoint[] tiltedBitmapCorners;
        SKRect tiltedBitmapBoundsRect;
        bool makeSelectRect = false;
        int minAngle = -45;
        int maxAngle = 45;
        float infoMiddleX, infoMiddleY;
        SKPath BSB2, tiltedBPath;
        SKPoint[] P = { };
        int rotateSign;
        bool moveSelectX = false;
        bool moveSelectY = false;
        bool showSelectionHandles = false;
        SKBitmap tiltedView;
        bool tiltAngleChanged = false;
        int scrollValueDefault = 5;
        int scrollValue = 5;
        bool collapse = false;
        bool showPixels = true;
        SKBitmap bitmapUpscaled;
        bool setUpscaledBitmap = false;

        private Dictionary<string, object> CanvasAttributes { get; set; }

        public async Task MedianJS(int k)
        {
            var bytes = sKBitmap.Bytes;
            var buffer = await JS.InvokeAsync<byte[]>("medianFilter", bytes,
                sKBitmap.Width, sKBitmap.Height, k);
            unsafe
            {
                fixed (byte* ptr = buffer)
                {
                    sKBitmap.SetPixels((IntPtr)ptr);
                }
            }
            AddBitmap(sKBitmap);
            skiaView.Invalidate();
        }

        public void ReloadBitmap()
        {
            AddBitmap(SService.originalBitmap);
            skiaView.Invalidate();
        }

        private void OnChangeShowPixels()
        {
            showPixels = !showPixels;
            skiaView.Invalidate();
        }

        public void UpdateBitmap(SKBitmap bitmap)
        {
            AddBitmap(bitmap);
        }

        public void ShowBitmap(SKBitmap bitmap)
        {
            sKBitmap = bitmap;
            setUpscaledBitmap = true;
            skiaView.Invalidate();
        }

        protected override async Task OnInitializedAsync()
        {
            SService.OnChange += StateHasChanged;
            dotNetHelper = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("WindowDPRHelper.callupdatePixelRatio", dotNetHelper);

            clickTime.Elapsed += (sender, e) => { isClick = false; };
            var openDbResult = await IndexedDbContext.OpenIndexedDb();
            await LoadImageToEdit();

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JS.InvokeVoidAsync("attachHandlers");
            }
        }

        private SKBitmap GetTiltedView2()
        {
            float ratio = (float)sKBitmap.Width / bitmapRect.Width;
            SKImageInfo info2 = new SKImageInfo((int)Math.Round(tiltedBitmapBoundsRect.Width * ratio),
                                                (int)Math.Round(tiltedBitmapBoundsRect.Height * ratio));
            
            SKBitmap surface2 = new SKBitmap(info2);
            using (SKCanvas canvas = new SKCanvas(surface2))
            {
                canvas.RotateDegrees(tiltAngle, surface2.Width / 2, surface2.Height / 2);
                canvas.DrawBitmap(sKBitmap, new SKPoint((info2.Width - sKBitmap.Width) / 2,
                                                        (info2.Height - sKBitmap.Height) / 2));
                canvas.RotateDegrees(-tiltAngle, surface2.Width / 2, surface2.Height / 2);
            }

            return surface2;
        }

        private void SetUpscaledBitmap()
        {
            float newSize = 800f;
            if (Math.Max(sKBitmap.Width, sKBitmap.Height) < newSize)
            {
                float scaleX = newSize / Math.Max(sKBitmap.Width, sKBitmap.Height);
                SKImageInfo infoX = new SKImageInfo((int)(sKBitmap.Width * scaleX),
                                                    (int)(sKBitmap.Height * scaleX));
                bitmapUpscaled = sKBitmap.Resize(infoX, SKFilterQuality.High);
            }
            else
                bitmapUpscaled = sKBitmap;
            setUpscaledBitmap = false;
        }

        private void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            infoMiddleX = info.Width / 2;
            infoMiddleY = info.Height / 2;
            canvas.Clear(SKColors.Black);

            SetBitmapRect(info);

            PanBitmapRect();

            scrollScaleChanged = false;

            SKPath unselected = new SKPath();
            unselected.AddRect(bitmapRect);

            if (tilting)
            {
                TiltUnselected(unselected);
                if (Math.Max(sKBitmap.Width, sKBitmap.Height) > 1000)
                {
                    canvas.RotateDegrees(tiltAngle, infoMiddleX, infoMiddleY);
                    canvas.DrawBitmap(sKBitmap, bitmapRect);
                    canvas.RotateDegrees(-tiltAngle, infoMiddleX, infoMiddleY);
                }
                else
                {
                    tiltedView = GetTiltedView2();
                    canvas.DrawBitmap(tiltedView, tiltedBitmapBoundsRect);
                }

            }
            else
            {
                if(showPixels)
                    canvas.DrawBitmap(sKBitmap, bitmapRect);
                else
                {
                    if (setUpscaledBitmap)
                        SetUpscaledBitmap();
                    canvas.DrawBitmap(bitmapUpscaled, bitmapRect);
                }

            }

            float crossLength = 10;
            SKPaint crossPaint = new SKPaint()
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2,
                Color = SKColors.White
            };
            canvas.DrawLine(infoMiddleX - crossLength, infoMiddleY, infoMiddleX + crossLength, infoMiddleY, crossPaint);
            canvas.DrawLine(infoMiddleX, infoMiddleY - crossLength, infoMiddleX, infoMiddleY + crossLength, crossPaint);

            if (tilting)
            {
                if (makeSelectRect)
                {
                    GenerateSelectRect(new SKPoint(infoMiddleX, infoMiddleY));
                    makeSelectRect = false;
                }
            }

            if (selectRect != SKRect.Empty)
            {
                if (!isDown)
                    AdjustSelection();

                unselected.AddRect(selectRect);

                unselected.FillType = SKPathFillType.EvenOdd;
                SKPaint unselectedPaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.Black.WithAlpha((byte)(0.5 * 255)),
                };
                canvas.DrawPath(unselected, unselectedPaint);

                SKColor selectionColor = SKColors.White;
                SKPaint paint = new SKPaint()
                {
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 3,
                    PathEffect = SKPathEffect.CreateDash(new float[] { 4, 4 }, 0),
                    Color = selectionColor
                };
                canvas.DrawRect(selectRect, paint);

                if (tilting)
                {
                    SKPaint directionPaint = new SKPaint()
                    {
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = 1,
                        Color = SKColors.White
                    };
                    canvas.DrawLine(0, infoMiddleY, info.Width, infoMiddleY, directionPaint);
                    canvas.DrawLine(infoMiddleX, 0, infoMiddleX, info.Height, directionPaint);

                }

                SKPaint circlePaint = new SKPaint()
                {
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true,
                    Color = selectionColor
                };
                if (showSelectionHandles)
                {
                    canvas.DrawCircle(selectRect.Left, selectRect.Top, selectCornerRadius, circlePaint);
                    canvas.DrawCircle(selectRect.Right, selectRect.Bottom, selectCornerRadius, circlePaint);
                    canvas.DrawCircle(selectRect.Left, selectRect.Bottom, selectCornerRadius, circlePaint);
                    canvas.DrawCircle(selectRect.Right, selectRect.Top, selectCornerRadius, circlePaint);
                }

                if (!tilting)
                {
                    SKPoint leftHandle = new SKPoint(selectRect.Left, selectRect.MidY);
                    SKPoint rightHandle = new SKPoint(selectRect.Right, selectRect.MidY);
                    SKPoint topHandle = new SKPoint(selectRect.MidX, selectRect.Top);
                    SKPoint bottomHandle = new SKPoint(selectRect.MidX, selectRect.Bottom);
                    if (showSelectionHandles)
                    {
                        canvas.DrawCircle(leftHandle, selectCornerRadius, circlePaint);
                        canvas.DrawCircle(rightHandle, selectCornerRadius, circlePaint);
                        canvas.DrawCircle(topHandle, selectCornerRadius, circlePaint);
                        canvas.DrawCircle(bottomHandle, selectCornerRadius, circlePaint);
                    }
                }
            }
            //Console.WriteLine("painted");
        }

        private void OnTiltAngleChange(ChangeEventArgs e)
        {
            tiltAngle = Convert.ToInt32(e.Value);
            tiltAngle = Math.Min(tiltAngle, maxAngle);
            tiltAngle = Math.Max(tiltAngle, minAngle);
            if (tiltAngle == 0)
                ResetTilt();
            else
            {
                tiltAngleChanged = true;
                TiltImage();
            }
        }

        private void OnMouseWheel(WheelEventArgs e)
        {
            if (e.DeltaY > 0)
            {
                scrollValue++;
            }
            else
            {
                scrollValue--;
                scrollValue = Math.Max(scrollValue, 0);
            }
            float c = 0.2f;
            int steps = scrollValueDefault;
            float a = ((1 - c) / (steps * steps));
            scrollScale = a * scrollValue * scrollValue + c;

            scrollScaleChanged = true;
            skiaView.Invalidate();
        }

        private void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == 1 && !tilting)
            {
                isMiddle = true;
                panRect.Left = (float)e.OffsetX;
                panRect.Top = (float)e.OffsetY;
            }      
            else if (e.Button == 0)
            {
                clickTime.Start();
                isDown = true;
                (float scaledCoordX, float scaledCoordY) = ScaleCoordinates((float)e.OffsetX, (float)e.OffsetY);
                SKPoint cursor = new SKPoint(scaledCoordX, scaledCoordY);
                float distToLeftTop = SKPoint.Distance(new SKPoint(selectRect.Left, selectRect.Top), cursor);
                float distToMidXTop = SKPoint.Distance(new SKPoint(selectRect.MidX, selectRect.Top), cursor);
                float distToRightTop = SKPoint.Distance(new SKPoint(selectRect.Right, selectRect.Top), cursor);
                float distToRightMidY = SKPoint.Distance(new SKPoint(selectRect.Right, selectRect.MidY), cursor);
                float distToRightBottom = SKPoint.Distance(new SKPoint(selectRect.Right, selectRect.Bottom), cursor);
                float distToMidXBottom = SKPoint.Distance(new SKPoint(selectRect.MidX, selectRect.Bottom), cursor);
                float distToLeftBottom = SKPoint.Distance(new SKPoint(selectRect.Left, selectRect.Bottom), cursor);
                float distToLeftMidY = SKPoint.Distance(new SKPoint(selectRect.Left, selectRect.MidY), cursor);

                if (distToLeftTop < selectCornerRadius)
                {
                    ReverseSelection();
                    SetSelectionOffset();
                }
                else if (distToMidXTop < selectCornerRadius)
                {
                    ReverseSelection();
                    SetSelectionOffset();
                    moveSelectY = true;
                }
                else if (distToRightTop < selectCornerRadius)
                {
                    MirrorSelectRect(selectRect.Left, selectRect.Bottom, selectRect.Right, selectRect.Top);
                    SetSelectionOffset();
                }
                else if (distToRightMidY < selectCornerRadius)
                {
                    moveSelectX = true;
                }
                else if (distToRightBottom < selectCornerRadius)
                {
                }
                else if (distToMidXBottom < selectCornerRadius)
                {
                    moveSelectY = true;
                }
                else if (distToLeftBottom < selectCornerRadius)
                {
                    MirrorSelectRect(selectRect.Right, selectRect.Top, selectRect.Left, selectRect.Bottom);
                    SetSelectionOffset();
                }
                else if (distToLeftMidY < selectCornerRadius)
                {
                    ReverseSelection();
                    SetSelectionOffset();
                    moveSelectX = true;
                }
                else
                {
                    SetSelectionStart(scaledCoordX, scaledCoordY);
                    SetSelectionOffset();
                }
            }
        }

        private void OnMouseMove(MouseEventArgs e)
        {
            (float scaledCoordX, float scaledCoordY) = ScaleCoordinates((float)e.OffsetX, (float)e.OffsetY);
            if (isDown)
            {
                SetSelectionEnd(scaledCoordX, scaledCoordY);
                SetSelectOriginal();

                skiaView.Invalidate();
            }

            if (isMiddle && !tilting)
            {
                panRect.Right = (float)e.OffsetX;
                panRect.Bottom = (float)e.OffsetY;
                skiaView.Invalidate();
            }
        }

        private void OnMouseUp(MouseEventArgs e)
        {
            isDown = false;
            isMiddle = false;
            moveSelectX = false;
            moveSelectY = false;
        }

        private void OnMouseOut(MouseEventArgs e)
        {
            clickTime.Stop();
            isDown = false;
            isMiddle = false;
            showSelectionHandles = false;
            skiaView.Invalidate();
        }

        private void OnClick(MouseEventArgs e)
        {
            clickTime.Stop();
            if (isClick)
            {
                selectRect = SKRect.Empty;
                skiaView.Invalidate();
            }
            isClick = true;
        }

        private void OnMouseOver(MouseEventArgs e)
        {
            showSelectionHandles = true;
            skiaView.Invalidate();
        }

        public void Dispose()
        {
            dotNetHelper?.Dispose();
        }

        private async Task SaveImage()
        {
            if (ImageId != null)
            {
                SService.originalBitmap = sKBitmap;
                await IDbManager.UpdateIDb2(sKBitmap, (int)ImageId, ImageName);
            }
        }

        private void ModalShowDownloadComponent()
        {
            var parameters = new ModalParameters();
            parameters.Add("passedBitmap", sKBitmap);
            Modal.Show<DownloadComponent>("Download image", parameters);
        }

        private async Task ModalShowResizeComponent()
        {
            var parameters = new ModalParameters();
            parameters.Add("OriginalWidth", sKBitmap.Width);
            parameters.Add("OriginalHeight", sKBitmap.Height);
            parameters.Add("bitmap", sKBitmap);

            var formModal = Modal.Show<ResizeComponent>("Resize image", parameters);
            var result = await formModal.Result;

            if (result.Cancelled)
            {
                //Console.WriteLine("Modal was cancelled");
            }
            else
            {
                //Console.WriteLine("Modal was accepted (resize)");
                AddBitmap((SKBitmap)result.Data);
                skiaView.Invalidate();
            }
        }

        private void SetBitmapRect(SKImageInfo info)
        {
            float scale = Math.Min((float)info.Width / sKBitmap.Width,
                                  (float)info.Height / sKBitmap.Height);
            float x = (info.Width - scale * scrollScale * sKBitmap.Width) / 2;
            float y = (info.Height - scale * scrollScale * sKBitmap.Height) / 2;
            float x2 = x + scale * scrollScale * sKBitmap.Width;
            float y2 = y + scale * scrollScale * sKBitmap.Height;

            bitmapRect = new SKRect(x, y, x2, y2);
        }

        public void ResetTilt()
        {
            tiltAngle = 0;
            tilting = false;
            skiaView.Invalidate();
        }

        public void TiltImage()
        {
            tilting = true;
            makeSelectRect = true;
            ResetPanOffset();
            ResetScale();
            skiaView.Invalidate();
        }

        public void FlipImage(bool isHorizontal)
        {
            AddBitmap(FlipBitmap(isHorizontal));
            if (selectRect != SKRect.Empty)
                FlipRect2(isHorizontal);
            ResetPanOffset();
            skiaView.Invalidate();
        }

        public void RotateImage(int sign)
        {
            rotateSign = sign;
            if (selectRect != SKRect.Empty)
                RotateRect();
            AddBitmap(RotateBitmap());
            ResetPanOffset();
            skiaView.Invalidate();
        }

        private void CropImage()
        {
            if (selectRect != SKRect.Empty)
            {
                SKMatrix scale, translate, result;
                float bitmapToViewRatio = (float)sKBitmap.Width / bitmapRect.Width;

                OrderSelectRect();

                translate = SKMatrix.CreateTranslation(-bitmapRect.Left, -bitmapRect.Top);
                scale = SKMatrix.CreateScale(bitmapToViewRatio, bitmapToViewRatio);

                SKRect cropRect = SKRect.Empty;
                result = SKMatrix.CreateIdentity();
                SKMatrix.PreConcat(ref result, scale);
                SKMatrix.PreConcat(ref result, translate);
                cropRect = result.MapRect(selectRect);

                float movX = -new List<float>() { cropRect.Left, cropRect.Right, 0 }.Min();
                float movY = -new List<float>() { cropRect.Top, cropRect.Bottom, 0 }.Min();
                result = SKMatrix.CreateIdentity();
                translate = SKMatrix.CreateTranslation(movX, movY);
                SKMatrix.PreConcat(ref result, translate);
                cropRect = result.MapRect(cropRect);

                SKRect dest = new SKRect(0, 0, cropRect.Width, cropRect.Height);
                SKBitmap croppedBitmap = new SKBitmap((int)cropRect.Width, (int)cropRect.Height);
                using (SKCanvas canvas = new SKCanvas(croppedBitmap))
                {
                    if (tilting)
                    {
                        int rBSize = 2 * Math.Max(sKBitmap.Width, sKBitmap.Height);
                        SKBitmap rotatedSKBitmap = new SKBitmap(rBSize, rBSize);
                        using (SKCanvas canvas2 = new SKCanvas(rotatedSKBitmap))
                        {
                            canvas2.Translate(movX, movY);
                            canvas2.RotateDegrees(tiltAngle, sKBitmap.Width / 2, sKBitmap.Height / 2);
                            canvas2.DrawBitmap(sKBitmap, new SKPoint(0, 0));
                            canvas2.RotateDegrees(-tiltAngle, sKBitmap.Width / 2, sKBitmap.Height / 2);
                        }
                        canvas.DrawBitmap(rotatedSKBitmap, cropRect, dest);
                        rotatedSKBitmap.Dispose();
                    }
                    else
                        canvas.DrawBitmap(sKBitmap, cropRect, dest);
                }

                AddBitmap(croppedBitmap);
                selectRect = SKRect.Empty;
                ResetPanOffset();
                if (scrollScale > 1)
                    ResetScale();
                if (tilting)
                    ResetTilt();
                skiaView.Invalidate();
            }
            //else
                //Console.WriteLine("No selection error message");
        }

        private void ResetPanOffset()
        {
            panOffset = new SKPoint(0, 0);
            scrollScaleChanged = true;
            skiaView.Invalidate();
        }

        private void ResetScale()
        {
            scrollValue = scrollValueDefault;
            scrollScale = 1;
            scrollScaleChanged = true;
            skiaView.Invalidate();
        }

        public async Task LoadImageToEdit()
        {
            ImageName = await LSManager.GetImageName();
            ImageId = await LSManager.GetImageId();
            if (ImageId != null)
            {
                imageResized2 = await IDbManager.FetchImageResized2((int)ImageId);
                if (imageResized2 != null)
                {
                }

                imageOriginal2 = await IDbManager.FetchImageOriginal2((int)ImageId);
                if (imageOriginal2 != null)
                {
                    sKBitmap = SKBitmap.Decode(imageOriginal2.Array);
                    //Console.WriteLine(sKBitmap.ColorType);
                    SService.originalBitmap = sKBitmap;
                    AddBitmap(sKBitmap);
                }
            }
        }

        [JSInvokable]
        public void OnKeyPressedJS(bool eCtrlKey, bool eShiftKey, string eCode)
        {
            if (eCode == "KeyZ")
            {
                if (eCtrlKey)
                {
                    if (eShiftKey)
                    {
                        currentIndex++;
                    }
                    else
                    {
                        currentIndex--;
                    }
                    currentIndex = Math.Max(0, currentIndex);
                    currentIndex = Math.Min(Math.Max(0, lastBitmaps.Count - 1), currentIndex);

                    sKBitmap = lastBitmaps[currentIndex];
                    setUpscaledBitmap = true;
                    SService.bitmap = sKBitmap;
                    skiaView.Invalidate();
                }
            }
            if (eCode == "KeyS")
            {
                SaveImage();
            }
        }

        [JSInvokable]
        public void SetWindowDPR(float pr)
        {
            windowDPR = pr;
        }

        private void PanBitmapRect()
        {
            float panX = panRect.Right - panRect.Left;
            float panY = panRect.Bottom - panRect.Top;
            panOffset.X += panX;
            panOffset.Y += panY;

            panRect.Left = panRect.Right;
            panRect.Top = panRect.Bottom;

            bitmapRect.Offset(panOffset.X, panOffset.Y);
        }

        private void TiltUnselected(SKPath unselected)
        {
            SKMatrix result = SKMatrix.CreateRotationDegrees(tiltAngle, infoMiddleX, infoMiddleY);
            unselected.Transform(result);
            tiltedBitmapCorners = unselected.Points;
            tiltedBitmapBoundsRect = unselected.Bounds;
        }

        private void SetUpNewSelectRect()
        {
            SetSelectionOffset();
            SetSelectOriginal();
        }

        private SKPoint? IntersectionPoint(SKPoint A, SKPoint B, SKPoint C, SKPoint D)
        {
            double a1 = B.Y - A.Y;
            double b1 = A.X - B.X;
            double c1 = a1 * (A.X) + b1 * (A.Y);

            double a2 = D.Y - C.Y;
            double b2 = C.X - D.X;
            double c2 = a2 * (C.X) + b2 * (C.Y);

            double determinant = a1 * b2 - a2 * b1;

            if (determinant == 0)
            {
                return null;
            }
            else
            {
                double x = (b2 * c1 - b1 * c2) / determinant;
                double y = (a1 * c2 - a2 * c1) / determinant;
                return new SKPoint((float)x, (float)y);
            }
        }

        private void GenerateSelectRect(SKPoint S)
        {
            SKPoint Atb = tiltedBitmapCorners[0];
            SKPoint Btb = tiltedBitmapCorners[1];
            SKPoint Ctb = tiltedBitmapCorners[2];
            SKPoint Dtb = tiltedBitmapCorners[3];
            SKPoint A = new SKPoint(tiltedBitmapBoundsRect.Left, tiltedBitmapBoundsRect.Top);
            SKPoint B = new SKPoint(tiltedBitmapBoundsRect.Right, tiltedBitmapBoundsRect.Top);
            SKPoint? ip1, ip2;
            if (tiltAngle > 0)
            {
                ip1 = IntersectionPoint(Atb, Btb, S, B);
                ip2 = IntersectionPoint(Atb, Dtb, S, A);
            }
            else
            {
                ip1 = IntersectionPoint(Atb, Btb, S, A);
                ip2 = IntersectionPoint(Btb, Ctb, S, B);
            }
            SKPoint startPoint = new SKPoint();
            SKPoint endPoint = new SKPoint();
            if (ip1 != null && ip2 != null)
            {
                float d1 = SKPoint.Distance((SKPoint)ip1, S);
                float d2 = SKPoint.Distance((SKPoint)ip2, S);

                if (d1 < d2)
                    startPoint = (SKPoint)ip1;
                else
                    startPoint = (SKPoint)ip2;

                SKPoint dV = S - startPoint;
                endPoint = S + dV;

                selectRect = new SKRect(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);
                OrderSelectRect();
                SetUpNewSelectRect();
            }
        }

        private void AdjustSelection()
        {
            if(rotateSelect)
            {
                float viewtoBitmapRatio = bitmapRect.Width / (float)sKBitmap.Width;
                SKMatrix result = SKMatrix.CreateIdentity();
                SKMatrix scale = SKMatrix.CreateScale(viewtoBitmapRatio, viewtoBitmapRatio);
                SKMatrix.PreConcat(ref result, scale);
                ShowSKMatrix(result, $"scale {viewtoBitmapRatio}");
                selectRect = result.MapRect(selectRect);

                selectOffset.X = selectRect.Left;
                selectOffset.Y = selectRect.Top;
                result = SKMatrix.CreateIdentity();
                SKMatrix translate = SKMatrix.CreateTranslation(bitmapRect.Left, bitmapRect.Top);
                SKMatrix.PreConcat(ref result, translate);
                ShowSKMatrix(result, $"translate {bitmapRect.Left} {bitmapRect.Top}");
                selectRect = result.MapRect(selectRect);

                SetSelectOriginal();
                rotateSelect = false;
            }
            selectRect.Left = selectRectOriginal.Left * scrollScale;
            selectRect.Top = selectRectOriginal.Top * scrollScale;
            selectRect.Right = selectRectOriginal.Right * scrollScale;
            selectRect.Bottom = selectRectOriginal.Bottom * scrollScale;

            selectRect.Right -= selectRect.Left;
            selectRect.Bottom -= selectRect.Top;
            selectRect.Left = 0;
            selectRect.Top = 0;

            selectOffset.X = selectOffsetOriginal.X * scrollScale;
            selectOffset.Y = selectOffsetOriginal.Y * scrollScale;

            selectRect.Left += bitmapRect.Left + selectOffset.X;
            selectRect.Top += bitmapRect.Top + selectOffset.Y;
            selectRect.Right += bitmapRect.Left + selectOffset.X;
            selectRect.Bottom += bitmapRect.Top + selectOffset.Y;
        }

        public void ShowSKMatrix(SKMatrix result, string title)
        {
            string str = $"{title}: ";
            foreach (var item in result.Values)
            {
                str += $"{item}, ";
            }
        }

        public SKBitmap FlipBitmap(bool isHorizontal)
        {
            SKBitmap flippedBitmap = new SKBitmap(sKBitmap.Width, sKBitmap.Height);

            using (SKCanvas canvas = new SKCanvas(flippedBitmap))
            {
                canvas.Clear();
                if (isHorizontal)
                { 
                    canvas.Translate(sKBitmap.Width, 0);
                    canvas.Scale(-1, 1);
                }
                else
                { 
                    canvas.Translate(0, sKBitmap.Height); 
                    canvas.Scale(1, -1);
                }
                canvas.DrawBitmap(sKBitmap, new SKPoint(0,0));
            }

            return flippedBitmap;
        }

        public void FlipRect2(bool isHorizontal)
        {
            SKMatrix translate1, translate2, translate3, scale;

            translate1 = SKMatrix.CreateTranslation(-bitmapRect.Left, -bitmapRect.Top);

            var result = SKMatrix.CreateIdentity();
            if (isHorizontal)
            { 
                translate2 = SKMatrix.CreateTranslation(bitmapRect.Width, 0);
                scale = SKMatrix.CreateScale(-1, 1);
            }
            else
            { 
                translate2 = SKMatrix.CreateTranslation(0, bitmapRect.Height);
                scale = SKMatrix.CreateScale(1, -1);
            }

            translate3 = SKMatrix.CreateTranslation(bitmapRect.Left, bitmapRect.Top);

            SKMatrix.PreConcat(ref result, translate3);
            SKMatrix.PreConcat(ref result, translate2);
            SKMatrix.PreConcat(ref result, scale);
            SKMatrix.PreConcat(ref result, translate1);
            selectRect = result.MapRect(selectRect);

            SetUpNewSelectRect();
        }

        public SKBitmap RotateBitmap()
        {
            SKBitmap rotatedBitmap = new SKBitmap(sKBitmap.Height, sKBitmap.Width);
            using (SKCanvas canvas = new SKCanvas(rotatedBitmap))
            {
                canvas.Clear();
                if (rotateSign > 0)
                {
                    canvas.Translate(sKBitmap.Height, 0);
                    canvas.RotateDegrees(90);
                }
                else
                { 
                    canvas.Translate(0, sKBitmap.Width);
                    canvas.RotateDegrees(-90);
                }
                canvas.DrawBitmap(sKBitmap, new SKPoint(0,0));
            }

            return rotatedBitmap;
        }

        public void RotateRect()
        {
            SKMatrix rotate, scale, translate1, translate2;
            SKMatrix result = SKMatrix.CreateIdentity();
            translate1 = SKMatrix.CreateTranslation(-bitmapRect.Left, -bitmapRect.Top);
            float bitmaptoViewRatio = (float)sKBitmap.Width / bitmapRect.Width;
            scale = SKMatrix.CreateScale(bitmaptoViewRatio, bitmaptoViewRatio);
            rotate = SKMatrix.CreateRotationDegrees(90);
            translate2 = SKMatrix.CreateTranslation(sKBitmap.Height, 0);

            SKMatrix.PreConcat(ref result, translate2);
            SKMatrix.PreConcat(ref result, rotate);
            SKMatrix.PreConcat(ref result, scale);
            SKMatrix.PreConcat(ref result, translate1);
            selectRect = result.MapRect(selectRect);

            OrderSelectRect();
            rotateSelect = true;
        }

        private void AddBitmap(SKBitmap bitmap)
        {
            SService.bitmap = bitmap;
            sKBitmap = bitmap;
            setUpscaledBitmap = true;
            if (currentIndex < lastBitmaps.Count - 1)
            {
                lastBitmaps.RemoveRange(currentIndex + 1, lastBitmaps.Count - currentIndex - 1);
                lastBitmaps.Add(bitmap);
                currentIndex++;
            }
            else
            {
                if (lastBitmaps.Count < imax)
                {
                    lastBitmaps.Add(bitmap);
                    currentIndex++;
                }
                else
                {
                    lastBitmaps = lastBitmaps.GetRange(1, lastBitmaps.Count - 1);
                    lastBitmaps.Add(bitmap);
                }
            }
        }

        private void OrderSelectRect()
        {
            if (selectRect.Left > selectRect.Right)
            {
                float tLeft = selectRect.Left;
                selectRect.Left = selectRect.Right;
                selectRect.Right = tLeft;
            }

            if (selectRect.Top > selectRect.Bottom)
            {
                float tTop = selectRect.Top;
                selectRect.Top = selectRect.Bottom;
                selectRect.Bottom = tTop;
            }
        }

        private void MirrorSelectRect(float left, float top, float right, float bottom)
        {
            SKRect oldselectRect = selectRect;
            selectRect.Left = left;
            selectRect.Top = top;
            selectRect.Right = right;
            selectRect.Bottom = bottom;

        }

        private (float scaledOffsetX, float scaledOffsetY) ScaleCoordinates(float eOffsetX, float eOffsetY)
        {
            return ((float)eOffsetX * windowDPR, (float)eOffsetY * windowDPR);
        }

        private void ReverseSelection()
        {
            float left = selectRect.Left;
            float top = selectRect.Top;

            selectRect.Left = selectRect.Right;
            selectRect.Right = left;
            selectRect.Top = selectRect.Bottom;
            selectRect.Bottom = top;
        }

        private void SetSelectionOffset()
        {
            selectOffset.X = selectRect.Left - bitmapRect.Left;
            selectOffset.Y = selectRect.Top - bitmapRect.Top;
        }

        private SKRect MakeSelectSymmetric(SKPoint S, SKPoint start)
        {
            SKPoint d = S - start;
            SKPoint end = S + d;
            return new SKRect(start.X, start.Y, end.X, end.Y);
        }

        private bool IsPointOnALine(SKPoint A, SKPoint B, SKPoint P)
        {
            float eps = 0.01f;
            float AP = SKPoint.Distance(A,P);
            float PB = SKPoint.Distance(P,B);
            float AB = SKPoint.Distance(A,B);

            return Math.Abs(AB - (AP + PB)) < eps;
        }

        private void SetSelectionStart(double offsetX, double offsetY)
        {
            if(tilting)
            {
                SetSelectionEnd(offsetX, offsetY);
            }
            else
            {
                selectRect.Left = (float)Math.Max(bitmapRect.Left, offsetX);
                selectRect.Top = (float)Math.Max(bitmapRect.Top, offsetY);

                selectRect.Left = (float)Math.Min(bitmapRect.Right, selectRect.Left);
                selectRect.Top = (float)Math.Min(bitmapRect.Bottom, selectRect.Top);
            }
        }

        private SKPoint CalcSelectRect(float offsetX, float offsetY)
        {
            SKPoint result = new SKPoint();
            P = new SKPoint[] { };

            SKPoint A = new SKPoint(offsetX, offsetY);
            SKPoint S = new SKPoint(infoMiddleX, infoMiddleY);

            float b = A.Y - A.X * (A.Y - S.Y) / (A.X - S.X);
            SKPoint B = new SKPoint(0, b);
            SKPoint B2 = new SKPoint(2 * S.X, b);

            BSB2 = new SKPath();
            BSB2.MoveTo(B);
            BSB2.LineTo(S);
            BSB2.LineTo(B2);

            tiltedBPath = new SKPath();
            tiltedBPath.MoveTo(tiltedBitmapCorners[0]);
            tiltedBPath.LineTo(tiltedBitmapCorners[1]);
            tiltedBPath.LineTo(tiltedBitmapCorners[2]);
            tiltedBPath.LineTo(tiltedBitmapCorners[3]);
            tiltedBPath.Close();

            SKPath intersection = tiltedBPath.Op(BSB2, SKPathOp.Intersect);
            if(intersection != null)
                P = intersection.Points;

            var pList = new List<(float dist, int index)>();
            if (P != null)
            {
                int i = 0;
                foreach (var point in P)
                {
                    if (IsPointOnALine(S, B, point) || IsPointOnALine(S, B2, point))
                    {
                        float d = SKPoint.Distance(S, point);
                        if(d != 0)
                            pList.Add((d, i)); 
                    }
                    i++;
                }
                if(pList.Count > 0)
                    result = P[pList.Min().index];
            }
            return result;
        }

        private void SetSelectionEnd(double offsetX, double offsetY)
        {
            if (tilting)
            {
                SKPoint start = CalcSelectRect((float)offsetX, (float)offsetY);
                SKRect newSelect = MakeSelectSymmetric(new SKPoint(infoMiddleX, infoMiddleY), start);
                if (newSelect != SKRect.Empty)
                {
                    selectRect = newSelect;
                    OrderSelectRect();
                    SetSelectionOffset();
                }
            }
            else
            {
                if (!moveSelectY)
                {
                    selectRect.Right = (float)Math.Min(bitmapRect.Right, offsetX);
                    selectRect.Right = (float)Math.Max(bitmapRect.Left, selectRect.Right);
                }
                if (!moveSelectX)
                {
                    selectRect.Bottom = (float)Math.Min(bitmapRect.Bottom, offsetY);
                    selectRect.Bottom = (float)Math.Max(bitmapRect.Top, selectRect.Bottom);
                }

            }
        }

        private void SetSelectOriginal()
        {
            selectRectOriginal.Left = selectRect.Left / scrollScale;
            selectRectOriginal.Top = selectRect.Top / scrollScale;
            selectRectOriginal.Right = selectRect.Right / scrollScale;
            selectRectOriginal.Bottom = selectRect.Bottom / scrollScale;

            selectOffsetOriginal.X = selectOffset.X / scrollScale;
            selectOffsetOriginal.Y = selectOffset.Y / scrollScale;
        }

    }
}
