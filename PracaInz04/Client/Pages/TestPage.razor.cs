using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using PracaInz04.Client.Controls;
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
        // initialize imageName in local storage on startup
        public string? ImageName { get; set; }
        public int? ImageId { get; set; }
        //public ImageResized imageResized { get; set; }
        public ImageData imageData { get; set; }
        public ImageResized imageResized { get; set; }
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
        // mozna uzyc selectRect.Location zmiast selectOffset 
        //(float x, float y) selectOffset;
        SKPoint selectOffset;
        //(float x, float y) selectOffsetOriginal;
        SKPoint selectOffsetOriginal;
        bool scrollScaleChanged = true;
        float distance;
        int selectCornerRadius = 12;
        private DotNetObjectReference<TestPage>? dotNetHelper;
        bool isMiddle = false;
        SKRect panRect = SKRect.Empty;
        //(float x, float y) panOffset;
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
        //SKPoint cursor;
        int rotateSign;
        bool moveSelectX = false;
        bool moveSelectY = false;
        bool showSelectionHandles = false;
        SKBitmap tiltedView;
        bool tiltAngleChanged = false;
        int scrollValueDefault = 5;
        int scrollValue = 5;

        private Dictionary<string, object> CanvasAttributes { get; set; }
        //new(){{ "width", "500" },{ "height", "500" }};

        protected override async Task OnInitializedAsync()
        {
            //Console.WriteLine("OnInitializedAsync");
            SService.OnChange += StateHasChanged;
            dotNetHelper = DotNetObjectReference.Create(this);
            //windowDPR = await JS.InvokeAsync<float>("windowDevicePixelRatio");
            await JS.InvokeVoidAsync("WindowDPRHelper.callupdatePixelRatio", dotNetHelper);

            //windowDPR = await JS.InvokeAsync<float>("windowDevicePixelRatio");
            //windowDPR = await JS.InvokeAsync<float>("callUpdatePixelRatio");

            //Console.WriteLine($"window.devicePixelRatio: {windowDPR}");
            //ImageName = SService.ImageName;
            clickTime.Elapsed += (sender, e) => { isClick = false; };
            var openDbResult = await IndexedDbContext.OpenIndexedDb();
            await LoadImageToEdit();

            //testSrc = await TestStuff();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                //await SetupBECanvas();
                await JS.InvokeVoidAsync("attachHandlers");
            }
            //else
            //{
            //    await LoadImageToEdit();
            //}
            //Console.WriteLine("OnAfterRenderAsync");
            //if(isSaving)
            //{
            //    await SaveImage();
            //    isSaving = false;
            //    progressModal.Close();
            //}
        }

        private SKBitmap GetTiltedView(SKImageInfo info)
        {
            float ratio = (float)sKBitmap.Width / bitmapRect.Width;
            SKImageInfo info2 = new SKImageInfo((int)Math.Round(info.Width * ratio), 
                                                (int)Math.Round(info.Height * ratio));
            // bitmapRect2.size = skbitmap.size (pixels)
            SKRect bitmapRect2 = new SKRect(bitmapRect.Left * ratio, bitmapRect.Top * ratio,
                                            bitmapRect.Right * ratio, bitmapRect.Bottom * ratio);
            // optional s1
            //bitmapRect2.Left = (float)Math.Ceiling(bitmapRect2.Left);
            //bitmapRect2.Top = (float)Math.Ceiling(bitmapRect2.Top);
            //bitmapRect2.Right = (float)Math.Ceiling(bitmapRect2.Right);
            //bitmapRect2.Bottom = (float)Math.Ceiling(bitmapRect2.Bottom);
            // optional e1
            SKBitmap surface2 = new SKBitmap(info2);
            using(SKCanvas canvas = new SKCanvas(surface2))
            {
                canvas.RotateDegrees(tiltAngle, info2.Rect.MidX, info2.Rect.MidY);
                canvas.DrawBitmap(sKBitmap, bitmapRect2);
                canvas.RotateDegrees(-tiltAngle, info2.Rect.MidX, info2.Rect.MidY);
            }

            return surface2;
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

        // private void OnPaintSurface(SKPaintSurfaceEventArgs e)
        private void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            infoMiddleX = info.Width / 2;
            infoMiddleY = info.Height / 2;
            //infoMiddleY = info.Rect.MidX;

            canvas.Clear();

            // nie obliczac bitmapRect od nowa, jezeli nie zmieniła się scroll skala
            //if (scrollScaleChanged || isMiddle)

            // calculate bitmapRect
            SetBitmapRect(info);

            // panning bitmapRect
            PanBitmapRect();

            scrollScaleChanged = false;

            // create unselected path, add bitmapRect
            SKPath unselected = new SKPath();
            unselected.AddRect(bitmapRect);

            // tilting - rotate view, tilt unselected path (bitmapRect)
            if (tilting)
            {
                TiltUnselected(unselected);
                if (Math.Max(sKBitmap.Width, sKBitmap.Height) > 1000)
                {
                    // tilting pixel grid too
                    canvas.RotateDegrees(tiltAngle, infoMiddleX, infoMiddleY);
                    //canvas.DrawBitmap(sKBitmap, bitmapRect, new SKPaint() { IsAntialias = true });
                    canvas.DrawBitmap(sKBitmap, bitmapRect);
                    canvas.RotateDegrees(-tiltAngle, infoMiddleX, infoMiddleY);
                }
                else
                {
                    // close to cropped
                    tiltedView = GetTiltedView2();
                    canvas.DrawBitmap(tiltedView, tiltedBitmapBoundsRect);
                }

                //if (tiltAngleChanged)
                //{
                //    tiltedView = GetTiltedView(info);
                //    tiltAngleChanged = false;
                //}

                //tiltedView = GetTiltedView(info);
                //canvas.DrawBitmap(tiltedView, new SKRect(0, 0, info.Width, info.Height));
            }
            else
            {
                canvas.DrawBitmap(sKBitmap, bitmapRect);
            }

            // draw cross in the middle
            float crossLength = 10;
            SKPaint crossPaint = new SKPaint()
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2,
                Color = SKColors.White
            };
            canvas.DrawLine(infoMiddleX - crossLength, infoMiddleY, infoMiddleX + crossLength, infoMiddleY, crossPaint);
            canvas.DrawLine(infoMiddleX, infoMiddleY - crossLength, infoMiddleX, infoMiddleY + crossLength, crossPaint);

            // original position of code
            // create unselected path, add bitmapRect
            //SKPath unselected = new SKPath();
            //unselected.AddRect(bitmapRect);

            // tilt unselected path (bitmapRect) + generate initial selectRect
            if (tilting)
            {
                // tilt unselected (bitmapRect)
                //TiltUnselected(unselected);

                // calculate selectRect
                if (makeSelectRect)
                {
                    GenerateSelectRect(new SKPoint(infoMiddleX, infoMiddleY));
                    makeSelectRect = false;
                }
            }

            // adjust and draw selectRect, add selectRect to unselected (path), draw unselected
            // draw level lines - tilting, draw handles
            if (selectRect != SKRect.Empty)
            {
                if (!isDown)
                    AdjustSelection();

                // add selectRect to unselected (path)
                unselected.AddRect(selectRect);

                // draw unselected
                unselected.FillType = SKPathFillType.EvenOdd;
                SKPaint unselectedPaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.Black.WithAlpha((byte)(0.5 * 255)),
                };
                canvas.DrawPath(unselected, unselectedPaint);

                // draw selectRect
                SKColor selectionColor = SKColors.White;
                SKPaint paint = new SKPaint()
                {
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 3,
                    PathEffect = SKPathEffect.CreateDash(new float[] { 4, 4 }, 0),
                    Color = selectionColor
                };
                canvas.DrawRect(selectRect, paint);

                // draw level lines, (optional - draw line form click to middle S)
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

                    // (optional - draw line form click to middle S)
                    //if (isDown)
                    //{
                    //    crossPaint.IsAntialias = true;
                    //    canvas.DrawLine(cursor.X, cursor.Y, infoMiddleX, infoMiddleY, crossPaint); 
                    //}
                }

                // draw corners of selectRect
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

                // draw horizontal/vertical selection handles
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
            Console.WriteLine("painted");
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
            //Console.WriteLine(e.Value);
        }

        // original OnMouseWheel
        private void OnMouseWheel1(WheelEventArgs e)
        {
            if (e.DeltaY > 0)
            {
                if (scrollScale < 2)
                {
                    scrollScale = (float)Math.Round(scrollScale + 0.1f, 1);
                }
            }
            else
            {
                if (scrollScale > 0.2)
                {
                    scrollScale = (float)Math.Round(scrollScale - 0.1f, 1);
                }
            }
            //Console.WriteLine($"OnMouseWheel: {e.DeltaY}, {scrollScale}");
            scrollScaleChanged = true;
            skiaView.Invalidate();
        }

        // OnMouseWheel2
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

            //scrollScale = 0.032f * scrollValue * scrollValue + 0.2f;
            
            //Console.WriteLine($"OnMouseWheel: {e.DeltaY}, {scrollScale}");
            scrollScaleChanged = true;
            skiaView.Invalidate();
        }

        private void OnMouseDown(MouseEventArgs e)
        {
            // middle mouse button - pan
            if (e.Button == 1 && !tilting)
            {
                isMiddle = true;
                panRect.Left = (float)e.OffsetX;
                panRect.Top = (float)e.OffsetY;
            } // left mouse button - select
            else if (e.Button == 0)
            {
                clickTime.Start();
                isDown = true;
                (float scaledCoordX, float scaledCoordY) = ScaleCoordinates((float)e.OffsetX, (float)e.OffsetY);
                //s2
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
                    // swap selectRect(Left, Top) with selectRect(Right, Bottom) -> OnMouseMove
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
                    //nothing? -> onmousemove
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
                    // set selection START - selectRect(Left, Top)
                    SetSelectionStart(scaledCoordX, scaledCoordY);
                    SetSelectionOffset();
                }
                Console.WriteLine($"OnMouseDown: moveSelectY : {moveSelectY}, moveSelectX : {moveSelectX}");
                //e2
                //s1
                //distance = Distance(selectRect.Right, selectRect.Bottom,
                //                    scaledCoordX, scaledCoordY);
                ////distance = SKPoint.Distance(new SKPoint(selectRect.Right, selectRect.Bottom), 
                ////                 new SKPoint(scaledCoordX, scaledCoordY));
                //if (distance > selectCornerRadius)
                //{
                //    // cursor NOT on selectRect(Right, Bottom)
                //    distance = Distance(selectRect.Left, selectRect.Top,
                //                        scaledCoordX, scaledCoordY);
                //    if (distance > selectCornerRadius)
                //    {
                //        // cursor NOT on selectRect(Left, Top)
                //        // set selection START - selectRect(Left, Top)
                //        SetSelectionStart(scaledCoordX, scaledCoordY);
                //        SetSelectionOffset();
                //    }
                //    else
                //    {
                //        // cursor on selectRect(Left, Top)
                //        // swap selectRect(Left, Top) with selectRect(Right, Bottom) -> OnMouseMove
                //        ReverseSelection();
                //        SetSelectionOffset();
                //    }
                //}
                //// else cursor on selectRect(Right, Bottom) -> OnMouseMove
                //e1
            }
        }

        private void OnMouseMove(MouseEventArgs e)
        {
            (float scaledCoordX, float scaledCoordY) = ScaleCoordinates((float)e.OffsetX, (float)e.OffsetY);
            if (isDown)
            {
                // cursor necessary for drawing a line cursor -> middle selection (tilting) 
                //cursor = new SKPoint(scaledCoordX, scaledCoordY);

                // set selection end - selectRect(Right, Bottom)
                SetSelectionEnd(scaledCoordX, scaledCoordY);
                SetSelectOriginal();

                //if (moveSelectX) 
                //{
                //    SetSelectionEnd(scaledCoordX, selectRect.Bottom);
                //    SetSelectOriginal();
                //}
                //else if (moveSelectY) 
                //{
                //    SetSelectionEnd(selectRect.Right, scaledCoordY);
                //    SetSelectOriginal();
                //}
                //else
                //{                
                //    // set selection end - selectRect(Right, Bottom)
                //    SetSelectionEnd(scaledCoordX, scaledCoordY);
                //    SetSelectOriginal();
                //}
                //Console.WriteLine($"OnMouseMove: scaledCoordX: {scaledCoordX}, scaledCoordY: {scaledCoordY}");
                skiaView.Invalidate();
            }

            //s1
            //if (bitmapRect.Contains(new SKPoint(scaledCoordX, scaledCoordY)))
            //    showSelectionHandles = true;
            //else
            //    showSelectionHandles = false;
            //e1

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
            //if (tilting)
            //    skiaView.Invalidate();
            //Console.WriteLine($"OnMouseUp: e.OffsetX: {e.OffsetX * windowDPR}, e.OffsetY: {e.OffsetY * windowDPR}");
        }

        private void OnMouseOut(MouseEventArgs e)
        {
            clickTime.Stop();
            //Console.WriteLine("timer stop");
            isDown = false;
            isMiddle = false;
            //s1
            showSelectionHandles = false;
            skiaView.Invalidate();
            //e1
            //Console.WriteLine("Out:");
        }

        // timer between down-up, short click resets rect
        private void OnClick(MouseEventArgs e)
        {
            clickTime.Stop();
            //Console.WriteLine("timer stop");
            //Console.WriteLine("Click");
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
                Console.WriteLine("SaveImage2 started");
                await IDbManager.UpdateIDb2(sKBitmap, (int)ImageId, ImageName);
                Console.WriteLine("SaveImage2 ended");
            }
        }

        private void ModalShowDownloadComponent()
        {
            SService.bitmap = sKBitmap;
            Modal.Show<DownloadComponent>("Download image");
        }

        private async Task ModalShowResizeComponent()
        {
            var parameters = new ModalParameters();
            parameters.Add("OriginalWidth", sKBitmap.Width);
            parameters.Add("OriginalHeight", sKBitmap.Height);

            SService.bitmap = sKBitmap;
            var formModal = Modal.Show<ResizeComponent>("Resize image", parameters);
            var result = await formModal.Result;

            if (result.Cancelled)
            {
                Console.WriteLine("Modal was cancelled");
            }
            else
            {
                Console.WriteLine("Modal was accepted (resize)");
                AddBitmap(SService.bitmap);
                skiaView.Invalidate();
            }
            //Modal.Show<ResizeComponent>("Resize image");
            //sKBitmap = SService.bitmap;
            //skiaView.Invalidate();
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
            //sKBitmap = TiltBitmap(degrees);
            //AddBitmap(sKBitmap);
            //ResetPanOffset();
            //ResetScale();

            //tiltAngle += degrees;
            //tiltAngle = degrees;
            tilting = true;
            makeSelectRect = true;
            ResetPanOffset();
            ResetScale();
            skiaView.Invalidate();
        }

        public void FlipImage(bool isHorizontal)
        {
            //sKBitmap = FlipBitmap(isHorizontal);
            AddBitmap(FlipBitmap(isHorizontal));
            if (selectRect != SKRect.Empty)
                FlipRect2(isHorizontal);
            ResetPanOffset();
            //ResetScale();

            skiaView.Invalidate();
        }

        public void RotateImage(int sign)
        {
            rotateSign = sign;
            if (selectRect != SKRect.Empty)
                RotateRect();
            //sKBitmap = RotateBitmap();
            AddBitmap(RotateBitmap());
            ResetPanOffset();
            //ResetScale();

            skiaView.Invalidate();
        }

        // private void CropImage() private void CropImage() private void CropImage() 

        // zeby zachowywalo piksele obrazkow o niskiej rozdzielczosci
        // - trzeba zwiekszyc rodzielczosc croppedBitmap i rotatedBitmap
        // using skmatrix transform + works + tilt cropping works
        private void CropImage()
        {
            if (selectRect != SKRect.Empty)
            {
                SKMatrix scale, translate, result;
                float bitmapToViewRatio = (float)sKBitmap.Width / bitmapRect.Width;

                // order selection start and end
                OrderSelectRect();

                // odjac poczatek bitmapy od selectRect
                translate = SKMatrix.CreateTranslation(-bitmapRect.Left, -bitmapRect.Top);
                // przeskalowac selectRect o bitmapToViewRatio
                scale = SKMatrix.CreateScale(bitmapToViewRatio, bitmapToViewRatio);

                SKRect cropRect = SKRect.Empty;
                result = SKMatrix.CreateIdentity();
                SKMatrix.PreConcat(ref result, scale);
                SKMatrix.PreConcat(ref result, translate);
                cropRect = result.MapRect(selectRect);

                // selectRect - bitmapRect moze byc ujemne dla tilted bitmap
                // np. ucinanie przy zaznaczeniu waskiego paska dla square tilted 45
                // trzeba przeniesc bitmapRect i cropRect do cwiartki x,y>=0
                float movX = -new List<float>() { cropRect.Left, cropRect.Right, 0 }.Min();
                float movY = -new List<float>() { cropRect.Top, cropRect.Bottom, 0 }.Min();
                //movX = (float)Math.Ceiling(movX);
                //movY = (float)Math.Ceiling(movX);
                result = SKMatrix.CreateIdentity();
                translate = SKMatrix.CreateTranslation(movX, movY);
                SKMatrix.PreConcat(ref result, translate);
                cropRect = result.MapRect(cropRect);

                //Console.WriteLine($"cR: ({cropRect.Left}, {cropRect.Top}), ({cropRect.Right}, {cropRect.Bottom})");
                //Console.WriteLine($"movX: {movX}, movY: {movY}");

                SKRect dest = new SKRect(0, 0, cropRect.Width, cropRect.Height);
                SKBitmap croppedBitmap = new SKBitmap((int)cropRect.Width, (int)cropRect.Height);
                //SKRect dest = new SKRect(0, 0, cropRect.Width*4, cropRect.Height*4);
                //SKBitmap croppedBitmap = new SKBitmap((int)cropRect.Width*4, (int)cropRect.Height*4);

                using (SKCanvas canvas = new SKCanvas(croppedBitmap))
                {
                    if (tilting)
                    {
                        // revise size
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
            else
                Console.WriteLine("No selection error message");
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

        // LoadImageToEdit2
        public async Task LoadImageToEdit()
        {
            ImageName = await LSManager.GetImageName();
            ImageId = await LSManager.GetImageId();
            if (ImageId != null)
            {
                imageResized2 = await IDbManager.FetchImageResized2((int)ImageId);
                if (imageResized2 != null)
                {
                    //sKBitmap = SKBitmap.Decode(imageResized.ImageArray);
                    //testSrc = await ImageProc.GetImageURL(imageResized.ImageArray);
                }

                // get original image
                imageOriginal2 = await IDbManager.FetchImageOriginal2((int)ImageId);
                if (imageOriginal2 != null)
                {
                    AddBitmap(SKBitmap.Decode(imageOriginal2.Array));
                    //testSrc2 = await ImageProc.GetImageURL(imageData.ImageArray);
                }
            }
        }

        [JSInvokable]
        public void OnKeyPressedJS(bool eCtrlKey, bool eShiftKey, string eCode)
        {
            //Console.WriteLine($"c#: e.CtrlKey: {eCtrlKey}, e.ShiftKey: {eShiftKey}, e.Key: {eCode}");
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
                    //Console.WriteLine($"currentIndex: {currentIndex}");
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
            //Console.WriteLine("SetWindowDPR");
        }

        ///////////////////////////////////////////////////////////////////////////
        
        // other methods
        private void PanBitmapRect()
        {
            float panX = panRect.Right - panRect.Left;
            float panY = panRect.Bottom - panRect.Top;
            panOffset.X += panX / scrollScale;
            panOffset.Y += panY / scrollScale;

            panRect.Left = panRect.Right;
            panRect.Top = panRect.Bottom;

            bitmapRect.Left += panOffset.X * scrollScale;
            bitmapRect.Top += panOffset.Y * scrollScale;
            bitmapRect.Right += panOffset.X * scrollScale;
            bitmapRect.Bottom += panOffset.Y * scrollScale;
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
            // Line AB represented as a1x + b1y = c1 
            double a1 = B.Y - A.Y;
            double b1 = A.X - B.X;
            double c1 = a1 * (A.X) + b1 * (A.Y);

            // Line CD represented as a2x + b2y = c2 
            double a2 = D.Y - C.Y;
            double b2 = C.X - D.X;
            double c2 = a2 * (C.X) + b2 * (C.Y);

            double determinant = a1 * b2 - a2 * b1;

            if (determinant == 0)
            {
                // The lines are parallel.
                //return new SKPoint(float.MaxValue, float.MaxValue);
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
            //Console.WriteLine("tiltedBitmapCorners:");
            //DisplayPoint(tiltedBitmapCorners[0]);

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

                //Console.WriteLine($"d1: {d1}, d2: {d2}");

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

        // manual manipulation 2 - works with: create sR > move sr(L,T) > scale
        private void AdjustSelection()
        {
            //
            if(rotateSelect)
            {
                //Console.WriteLine("test AdjustSelection > rotateSelect");
                float viewtoBitmapRatio = bitmapRect.Width / (float)sKBitmap.Width;
                SKMatrix result = SKMatrix.CreateIdentity();
                SKMatrix scale = SKMatrix.CreateScale(viewtoBitmapRatio, viewtoBitmapRatio);
                //result.PreConcat(scale);
                SKMatrix.PreConcat(ref result, scale);
                ShowSKMatrix(result, $"scale {viewtoBitmapRatio}");
                selectRect = result.MapRect(selectRect);

                selectOffset.X = selectRect.Left;
                selectOffset.Y = selectRect.Top;
                //sR.location
                //selectRect.Location = new SKPoint(selectRect.Left, selectRect.Top);
                //

                result = SKMatrix.CreateIdentity();
                SKMatrix translate = SKMatrix.CreateTranslation(bitmapRect.Left, bitmapRect.Top);
                //result.PreConcat(translate);
                SKMatrix.PreConcat(ref result, translate);
                ShowSKMatrix(result, $"translate {bitmapRect.Left} {bitmapRect.Top}");
                selectRect = result.MapRect(selectRect);

                SetSelectOriginal();
                rotateSelect = false;
            }
            //

            // selectRectOriginal jest dla scrollscale = 1
            // skalowanie selectRect
            selectRect.Left = selectRectOriginal.Left * scrollScale;
            selectRect.Top = selectRectOriginal.Top * scrollScale;
            selectRect.Right = selectRectOriginal.Right * scrollScale;
            selectRect.Bottom = selectRectOriginal.Bottom * scrollScale;

            // potrzebne do przeniesienia selectRect do poczatku bitmapRect
            // tu pewnie problem ze zmianą selekcji przy zaznaczeniu SE -> NW
            selectRect.Right -= selectRect.Left;
            selectRect.Bottom -= selectRect.Top;
            selectRect.Left = 0;
            selectRect.Top = 0;

            // skalowanie offsetu selectRect wzgledem poczatku bitmapRect
            selectOffset.X = selectOffsetOriginal.X * scrollScale;
            selectOffset.Y = selectOffsetOriginal.Y * scrollScale;

            // przeniesienie selectRect do poczatku bitmapRect + selectOffset
            selectRect.Left += bitmapRect.Left + selectOffset.X;
            selectRect.Top += bitmapRect.Top + selectOffset.Y;
            selectRect.Right += bitmapRect.Left + selectOffset.X;
            selectRect.Bottom += bitmapRect.Top + selectOffset.Y;
        }

        public bool PointInRectangle(SKPoint M, SKPoint[] r)
        {
            //var A = new SKPoint(r.Left, r.Top);
            //var B = new SKPoint(r.Right, r.Top);
            //var C = new SKPoint(r.Left, r.Bottom);

            var A = r[0];
            var B = r[1];
            var C = r[2];

            var AB = MakeVector(A, B);
            var AM = MakeVector(A, M);
            var BC = MakeVector(B, C);
            var BM = MakeVector(B, M);

            var dotABAM = DotProduct(AB, AM);
            var dotABAB = DotProduct(AB, AB);
            var dotBCBM = DotProduct(BC, BM);
            var dotBCBC = DotProduct(BC, BC);

            return 0 <= dotABAM && dotABAM <= dotABAB && 0 <= dotBCBM && dotBCBM <= dotBCBC;
        }

        public SKPoint MakeVector(SKPoint p1, SKPoint p2)
        {
            return new SKPoint(p2.X - p1.X, p2.Y - p1.Y);
        }

        public float DotProduct(SKPoint u, SKPoint v)
        {
            return u.X * v.X + u.Y * v.Y;
        }

        public void ShowSKMatrix(SKMatrix result, string title)
        {
            string str = $"{title}: ";
            foreach (var item in result.Values)
            {
                str += $"{item}, ";
            }
            //Console.WriteLine(str);
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

        // using skmatrix transforms
        public void FlipRect2(bool isHorizontal)
        {
            SKMatrix translate1, translate2, translate3, scale;

            //selectRect.Left -= bitmapRect.Left;
            //selectRect.Top -= bitmapRect.Top;
            //selectRect.Right -= bitmapRect.Left;
            //selectRect.Bottom -= bitmapRect.Top;
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

            //selectRect.Left += bitmapRect.Left;
            //selectRect.Top += bitmapRect.Top;
            //selectRect.Right += bitmapRect.Left;
            //selectRect.Bottom += bitmapRect.Top;
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
                //canvas.Translate(sKBitmap.Height, 0);
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
                //canvas.RotateDegrees(rotateSign * 90);
                canvas.DrawBitmap(sKBitmap, new SKPoint(0,0));
            }

            return rotatedBitmap;
        }

        // both RotateRect()s dont work properly for clicking rotate fast 
        // skmatrix transforms
        public void RotateRect()
        {
            SKMatrix rotate, scale, translate1, translate2;
            SKMatrix result = SKMatrix.CreateIdentity();
            //SKMatrix translate = SKMatrix.CreateTranslation(-selectOffset.X, -selectOffset.Y);
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
            sKBitmap = bitmap;
            //int imax = 5;
            if(currentIndex < lastBitmaps.Count - 1)
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
            //selectRect.Right = offsetX;
            //selectRect.Bottom = offsetY;
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
            // seemed to work without se1, except for: create sR > edit sr(L,T) > zoom in/out
            //s1
            //selectRectOriginal.Left = selectRectOriginal.Right;
            //selectRectOriginal.Right = left;
            //selectRectOriginal.Top = selectRectOriginal.Bottom;
            //selectRectOriginal.Bottom = top;
            //e1
        }

        private void SetSelectionOffset()
        {
            selectOffset.X = selectRect.Left - bitmapRect.Left;
            selectOffset.Y = selectRect.Top - bitmapRect.Top;
            //Console.WriteLine($"b:({bitmapRect.Left}, {bitmapRect.Top})\n"+
            //$"s:({selectRect.Left}, {selectRect.Top})\n"+ 
            //$"o: ({selectOffset.X}, {selectOffset.Y}))");
        }

        private void ModifySelectStart(SKPoint S)
        {
            SKPoint d = S - new SKPoint(selectRect.Right, selectRect.Bottom);
            SKPoint point = S + d;
            selectRect.Left = point.X;
            selectRect.Top = point.Y;
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

            //Console.WriteLine($"IsPointOnALine: {AB}, {AP + PB}, {AB == AP + PB}");
            //Console.WriteLine($"IsPointOnALine: {AB}, {AP + PB}, {Math.Abs(AB - (AP + PB)) < eps}");
            //return AB == AP + PB;
            return Math.Abs(AB - (AP + PB)) < eps;
        }

        // SetSelectionStart3 + works
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
            //SKPoint B2 = new SKPoint(2 * S.X, 0);
            SKPoint B2 = new SKPoint(2 * S.X, b);

            //SKPath BSB2 = new SKPath();
            BSB2 = new SKPath();
            BSB2.MoveTo(B);
            BSB2.LineTo(S);
            BSB2.LineTo(B2);

            //SKPath tiltedBPath = new SKPath();
            tiltedBPath = new SKPath();
            tiltedBPath.MoveTo(tiltedBitmapCorners[0]);
            tiltedBPath.LineTo(tiltedBitmapCorners[1]);
            tiltedBPath.LineTo(tiltedBitmapCorners[2]);
            tiltedBPath.LineTo(tiltedBitmapCorners[3]);
            //tiltedBPath.LineTo(tiltedBitmapCorners[0]);
            tiltedBPath.Close();

            //SKPath intersection = BSB2.Op(tiltedBPath, SKPathOp.Intersect);
            SKPath intersection = tiltedBPath.Op(BSB2, SKPathOp.Intersect);
            if(intersection != null)
                P = intersection.Points;

            //s3
            var pList = new List<(float dist, int index)>();
            if (P != null)
            {
                int i = 0;
                foreach (var point in P)
                {
                    // if point is between BS or B2S add to pList
                    if (IsPointOnALine(S, B, point) || IsPointOnALine(S, B2, point))
                    {
                        float d = SKPoint.Distance(S, point);
                        if(d != 0)
                            pList.Add((d, i)); 
                    }
                    i++;
                }
                //pList.Sort();

                if(pList.Count > 0)
                    result = P[pList.Min().index];
            }
            //e3
            return result;
        }

        // SetSelectionEnd4 + works latest 3
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
                    //Console.WriteLine("PointInRectangle End: true");
                }
                //else { Console.WriteLine("PointInRectangle End: false"); }
            }
            else
            {
                //selectRect.Right = (float)Math.Min(bitmapRect.Right, offsetX);
                //selectRect.Bottom = (float)Math.Min(bitmapRect.Bottom, offsetY);

                //selectRect.Right = (float)Math.Max(bitmapRect.Left, selectRect.Right);
                //selectRect.Bottom = (float)Math.Max(bitmapRect.Top, selectRect.Bottom);

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
