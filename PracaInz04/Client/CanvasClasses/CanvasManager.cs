﻿using Blazored.Modal;
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

namespace PracaInz04.Client.CanvasClasses
{
    public class CanvasManager
    {
        //    [Inject]
        //    StateService SService { get; set; }
        //    [Inject]
        //    LocalStorageManager LSManager { get; set; }
        //    [Inject]
        //    IndexedDbManager IDbManager { get; set; }
        //    [Inject]
        //    IndexedDbContext IndexedDbContext { get; set; }
        //    [Inject]
        //    NavigationManager NavigationManager { get; set; }
        //    [Inject]
        //    ImageProcessing ImageProc { get; set; }
        //    [Inject]
        //    IJSRuntime JS { get; set; }

        //    [CascadingParameter] public IModalService Modal { get; set; }
        //    public IModalReference progressModal { get; set; }
        //    // initialize imageName in local storage on startup
        //    public string? ImageName { get; set; }
        //    public int? ImageId { get; set; }
        //    //public ImageResized imageResized { get; set; }
        //    public ImageData imageData { get; set; }
        //    public ImageResized imageResized { get; set; }
        //    public ImageOriginal2 imageOriginal2 { get; set; }
        //    public ImageResized2 imageResized2 { get; set; }
        //    public SKBitmap sKBitmap { get; set; }

        //    public string testSrc { get; set; }
        //    public string testSrc2 { get; set; }
        //    SKCanvasView skiaView = null!;
        //    float scrollScale = 1;
        //    bool isDown = false;
        //    SKRect selectRect = SKRect.Empty;
        //    SKRect selectRectOriginal = SKRect.Empty;
        //    System.Timers.Timer clickTime = new System.Timers.Timer(100);
        //    bool isClick = true;
        //    SKRect bitmapRect = SKRect.Empty;
        //    float windowDPR = 1;
        //    // mozna uzyc selectRect.Location zmiast selectOffset 
        //    //(float x, float y) selectOffset;
        //    SKPoint selectOffset;
        //    //(float x, float y) selectOffsetOriginal;
        //    SKPoint selectOffsetOriginal;
        //    bool scrollScaleChanged = true;
        //    float distance;
        //    int selectCornerRadius = 12;
        //    //private DotNetObjectReference<TestPage>? dotNetHelper;
        //    bool isMiddle = false;
        //    SKRect panRect = SKRect.Empty;
        //    //(float x, float y) panOffset;
        //    SKPoint panOffset;
        //    List<SKBitmap> lastBitmaps = new List<SKBitmap>();
        //    int currentIndex = -1;
        //    int imax = 10;
        //    bool rotateSelect = false;
        //    float tiltAngle = 0;
        //    bool tilting = false;
        //    SKPoint[] tiltedBitmapCorners;
        //    SKRect tiltedBitmapRect;
        //    bool makeSelectRect = false;
        //    int minAngle = -45;
        //    int maxAngle = 45;
        //    float infoMiddleX, infoMiddleY;
        //    SKPath BSB2, tiltedBPath;
        //    SKPoint[] P = { };
        //    SKPoint cursor;

        //    private Dictionary<string, object> CanvasAttributes { get; set; }
        //    //new(){{ "width", "500" },{ "height", "500" }};

        //    //private async Task Progress(Func<Task> method, string message = "Progress...")
        //    //{
        //    //    var options = new ModalOptions()
        //    //    {
        //    //        HideCloseButton = true
        //    //    };
        //    //    var formModal = Modal.Show<ProgressComponent>(message, options);
        //    //    await Task.Delay(1);
        //    //    await method();
        //    //    formModal.Close();
        //    //    await Task.Delay(1); 
        //    //}

        //    // SaveImage2
        //    private async Task SaveImage()
        //    {
        //        if (ImageId != null)
        //        {
        //            Console.WriteLine("SaveImage2 started");
        //            await IDbManager.UpdateIDb2(sKBitmap, (int)ImageId, ImageName);
        //            Console.WriteLine("SaveImage2 ended");
        //        }
        //    }

        //    private void ModalShowDownloadComponent()
        //    {
        //        SService.bitmap = sKBitmap;
        //        Modal.Show<DownloadComponent>("Download image");
        //    }

        //    private async Task ModalShowResizeComponent()
        //    {
        //        var parameters = new ModalParameters();
        //        parameters.Add("OriginalWidth", sKBitmap.Width);
        //        parameters.Add("OriginalHeight", sKBitmap.Height);

        //        SService.bitmap = sKBitmap;
        //        var formModal = Modal.Show<ResizeComponent>("Resize image", parameters);
        //        var result = await formModal.Result;

        //        if (result.Cancelled)
        //        {
        //            Console.WriteLine("Modal was cancelled");
        //        }
        //        else
        //        {
        //            Console.WriteLine("Modal was accepted (resize)");
        //            AddBitmap(SService.bitmap);
        //            skiaView.Invalidate();
        //        }
        //        //Modal.Show<ResizeComponent>("Resize image");
        //        //sKBitmap = SService.bitmap;
        //        //skiaView.Invalidate();
        //    }

        //    private void SetUpNewSelectRect()
        //    {
        //        SetSelectionOffset();
        //        SetSelectOriginal();
        //    }

        //    private void OnTiltAngleChange(ChangeEventArgs e)
        //    {
        //        tiltAngle = Convert.ToInt32(e.Value);
        //        tiltAngle = Math.Min(tiltAngle, maxAngle);
        //        tiltAngle = Math.Max(tiltAngle, minAngle);
        //        if (tiltAngle == 0)
        //            ResetTilt();
        //        else
        //            TiltImage();
        //        //Console.WriteLine(e.Value);
        //    }

        //    //private void DisplayPoint(SKPoint p)
        //    //{
        //    //    Console.WriteLine($"({p.X}, {p.Y})");
        //    //    Console.WriteLine($"({p.X}, {p.Y})");
        //    //}

        //    private SKPoint? IntersectionPoint(SKPoint A, SKPoint B, SKPoint C, SKPoint D)
        //    {
        //        // Line AB represented as a1x + b1y = c1 
        //        double a1 = B.Y - A.Y;
        //        double b1 = A.X - B.X;
        //        double c1 = a1 * (A.X) + b1 * (A.Y);

        //        // Line CD represented as a2x + b2y = c2 
        //        double a2 = D.Y - C.Y;
        //        double b2 = C.X - D.X;
        //        double c2 = a2 * (C.X) + b2 * (C.Y);

        //        double determinant = a1 * b2 - a2 * b1;

        //        if (determinant == 0)
        //        {
        //            // The lines are parallel.
        //            //return new SKPoint(float.MaxValue, float.MaxValue);
        //            return null;
        //        }
        //        else
        //        {
        //            double x = (b2 * c1 - b1 * c2) / determinant;
        //            double y = (a1 * c2 - a2 * c1) / determinant;
        //            return new SKPoint((float)x, (float)y);
        //        }
        //    }

        //    private void GenerateSelectRect(SKPoint S)
        //    {
        //        //Console.WriteLine("tiltedBitmapCorners:");
        //        //DisplayPoint(tiltedBitmapCorners[0]);

        //        SKPoint Atb = tiltedBitmapCorners[0];
        //        SKPoint Btb = tiltedBitmapCorners[1];
        //        SKPoint Ctb = tiltedBitmapCorners[2];
        //        SKPoint Dtb = tiltedBitmapCorners[3];
        //        SKPoint A = new SKPoint(tiltedBitmapRect.Left, tiltedBitmapRect.Top);
        //        SKPoint B = new SKPoint(tiltedBitmapRect.Right, tiltedBitmapRect.Top);
        //        SKPoint? ip1, ip2;
        //        if (tiltAngle > 0)
        //        {
        //            ip1 = IntersectionPoint(Atb, Btb, S, B);
        //            ip2 = IntersectionPoint(Atb, Dtb, S, A);
        //        }
        //        else
        //        {
        //            ip1 = IntersectionPoint(Atb, Btb, S, A);
        //            ip2 = IntersectionPoint(Btb, Ctb, S, B);
        //        }
        //        SKPoint startPoint = new SKPoint();
        //        SKPoint endPoint = new SKPoint();
        //        if (ip1 != null && ip2 != null)
        //        {
        //            float d1 = SKPoint.Distance((SKPoint)ip1, S);
        //            float d2 = SKPoint.Distance((SKPoint)ip2, S);

        //            //Console.WriteLine($"d1: {d1}, d2: {d2}");

        //            if (d1 < d2)
        //                startPoint = (SKPoint)ip1;
        //            else
        //                startPoint = (SKPoint)ip2;

        //            SKPoint dV = S - startPoint;
        //            endPoint = S + dV;

        //            selectRect = new SKRect(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);
        //            OrderSelectRect();
        //            SetUpNewSelectRect();
        //        }
        //    }

        //    private void OnPaintSurface(SKPaintSurfaceEventArgs e)
        //    {
        //        SKImageInfo info = e.Info;
        //        SKSurface surface = e.Surface;
        //        SKCanvas canvas = surface.Canvas;
        //        infoMiddleX = info.Width / 2;
        //        infoMiddleY = info.Height / 2;

        //        canvas.Clear();

        //        // nie obliczac od nowa, jezeli nie zmieniła się scroll skala
        //        //if (scrollScaleChanged || isMiddle)
        //        //{

        //        // calculate bitmapRect
        //        float scale = Math.Min((float)info.Width / sKBitmap.Width,
        //                              (float)info.Height / sKBitmap.Height);
        //        float x = (info.Width - scale * scrollScale * sKBitmap.Width) / 2;
        //        float y = (info.Height - scale * scrollScale * sKBitmap.Height) / 2;
        //        float x2 = x + scale * scrollScale * sKBitmap.Width;
        //        float y2 = y + scale * scrollScale * sKBitmap.Height;
        //        bitmapRect = new SKRect(x, y, x2, y2);

        //        // panning
        //        float panX = panRect.Right - panRect.Left;
        //        float panY = panRect.Bottom - panRect.Top;
        //        panOffset.X += panX / scrollScale;
        //        panOffset.Y += panY / scrollScale;

        //        panRect.Left = panRect.Right;
        //        panRect.Top = panRect.Bottom;

        //        bitmapRect.Left += panOffset.X * scrollScale;
        //        bitmapRect.Top += panOffset.Y * scrollScale;
        //        bitmapRect.Right += panOffset.X * scrollScale;
        //        bitmapRect.Bottom += panOffset.Y * scrollScale;

        //        //}
        //        scrollScaleChanged = false;

        //        // tilting - rotate bitmap
        //        //if(tiltAngle != 0)
        //        if (tilting)
        //        {
        //            canvas.RotateDegrees(tiltAngle, infoMiddleX, infoMiddleY);
        //            canvas.DrawBitmap(sKBitmap, bitmapRect, new SKPaint() { IsAntialias = true });
        //            canvas.RotateDegrees(-tiltAngle, infoMiddleX, infoMiddleY);
        //            //tiltAngle = 0;
        //        }
        //        else
        //        {
        //            canvas.DrawBitmap(sKBitmap, bitmapRect);
        //        }

        //        // draw cross in the middle
        //        float crossLength = 10;
        //        SKPaint crossPaint = new SKPaint()
        //        {
        //            Style = SKPaintStyle.Stroke,
        //            StrokeWidth = 2,
        //            Color = SKColors.White
        //        };
        //        canvas.DrawLine(infoMiddleX - crossLength, infoMiddleY, infoMiddleX + crossLength, infoMiddleY, crossPaint);
        //        canvas.DrawLine(infoMiddleX, infoMiddleY - crossLength, infoMiddleX, infoMiddleY + crossLength, crossPaint);


        //        // create unselected path, add bitmapRect
        //        SKPath unselected = new SKPath();
        //        unselected.AddRect(bitmapRect);
        //        // tilt unselected path (bitmapRect)
        //        if (tilting)
        //        {
        //            // tilt unselected (bitmapRect)
        //            SKMatrix result = SKMatrix.CreateRotationDegrees(tiltAngle, infoMiddleX, infoMiddleY);
        //            unselected.Transform(result);
        //            tiltedBitmapCorners = unselected.Points;
        //            tiltedBitmapRect = unselected.Bounds;
        //            // calculate selectRect
        //            if (makeSelectRect)
        //            {
        //                GenerateSelectRect(new SKPoint(infoMiddleX, infoMiddleY));
        //                makeSelectRect = false;
        //            }
        //        }

        //        // adjust and draw selectRect, add selectRect to unselected (path)
        //        if (selectRect != SKRect.Empty)
        //        {
        //            if (!isDown)
        //                AdjustSelection();

        //            //Console.WriteLine($"{selectRect.Left}, {selectRect.Top}");

        //            SKColor selectionColor = SKColors.White;
        //            SKPaint paint = new SKPaint()
        //            {
        //                Style = SKPaintStyle.Stroke,
        //                StrokeWidth = 3,
        //                PathEffect = SKPathEffect.CreateDash(new float[] { 4, 4 }, 0),
        //                Color = selectionColor
        //            };
        //            canvas.DrawRect(selectRect, paint);

        //            // add selectRect to unselected (path)
        //            unselected.AddRect(selectRect);
        //            unselected.FillType = SKPathFillType.EvenOdd;
        //            SKPaint unselectedPaint = new SKPaint
        //            {
        //                Style = SKPaintStyle.Fill,
        //                Color = SKColors.Black.WithAlpha((byte)(0.5 * 255)),
        //            };
        //            canvas.DrawPath(unselected, unselectedPaint);

        //            //s1 draw line form click to middle S and draw level lines
        //            if (tilting)
        //            {
        //                SKPaint directionPaint = new SKPaint()
        //                {
        //                    Style = SKPaintStyle.Stroke,
        //                    StrokeWidth = 1,
        //                    Color = SKColors.White
        //                };
        //                canvas.DrawLine(0, infoMiddleY, info.Width, infoMiddleY, directionPaint);
        //                canvas.DrawLine(infoMiddleX, 0, infoMiddleX, info.Height, directionPaint);
        //                //if (isDown)
        //                //{
        //                //    crossPaint.IsAntialias = true;
        //                //    canvas.DrawLine(cursor.X, cursor.Y, infoMiddleX, infoMiddleY, crossPaint); 
        //                //}
        //            }
        //            //e1

        //            SKPaint circlePaint = new SKPaint()
        //            {
        //                Style = SKPaintStyle.Fill,
        //                IsAntialias = true,
        //                Color = selectionColor
        //            };
        //            // draw corners of selectRect
        //            canvas.DrawCircle(selectRect.Left, selectRect.Top, selectCornerRadius, circlePaint);
        //            canvas.DrawCircle(selectRect.Right, selectRect.Bottom, selectCornerRadius, circlePaint);
        //            //canvas.DrawCircle(selectRect.Left, selectRect.Bottom, selectCornerRadius, circlePaint);
        //            //canvas.DrawCircle(selectRect.Right, selectRect.Top, selectCornerRadius, circlePaint);
        //        }
        //        Console.WriteLine("painted");
        //    }

        //    // manual manipulation 2 - works with: create sR > move sr(L,T) > scale
        //    private void AdjustSelection()
        //    {
        //        //
        //        if (rotateSelect)
        //        {
        //            //Console.WriteLine("test AdjustSelection > rotateSelect");
        //            float viewtoBitmapRatio = bitmapRect.Width / (float)sKBitmap.Width;
        //            SKMatrix result = SKMatrix.CreateIdentity();
        //            SKMatrix scale = SKMatrix.CreateScale(viewtoBitmapRatio, viewtoBitmapRatio);
        //            //result.PreConcat(scale);
        //            SKMatrix.PreConcat(ref result, scale);
        //            ShowSKMatrix(result, $"scale {viewtoBitmapRatio}");
        //            selectRect = result.MapRect(selectRect);

        //            selectOffset.X = selectRect.Left;
        //            selectOffset.Y = selectRect.Top;
        //            //sR.location
        //            //selectRect.Location = new SKPoint(selectRect.Left, selectRect.Top);
        //            //

        //            result = SKMatrix.CreateIdentity();
        //            SKMatrix translate = SKMatrix.CreateTranslation(bitmapRect.Left, bitmapRect.Top);
        //            //result.PreConcat(translate);
        //            SKMatrix.PreConcat(ref result, translate);
        //            ShowSKMatrix(result, $"translate {bitmapRect.Left} {bitmapRect.Top}");
        //            selectRect = result.MapRect(selectRect);

        //            SetSelectOriginal();
        //            rotateSelect = false;
        //        }
        //        //

        //        // selectRectOriginal jest dla scrollscale = 1
        //        // skalowanie selectRect
        //        selectRect.Left = selectRectOriginal.Left * scrollScale;
        //        selectRect.Top = selectRectOriginal.Top * scrollScale;
        //        selectRect.Right = selectRectOriginal.Right * scrollScale;
        //        selectRect.Bottom = selectRectOriginal.Bottom * scrollScale;

        //        // potrzebne do przeniesienia selectRect do poczatku bitmapRect
        //        // tu pewnie problem ze zmianą selekcji przy zaznaczeniu SE -> NW
        //        selectRect.Right -= selectRect.Left;
        //        selectRect.Bottom -= selectRect.Top;
        //        selectRect.Left = 0;
        //        selectRect.Top = 0;

        //        // skalowanie offsetu selectRect wzgledem poczatku bitmapRect
        //        selectOffset.X = selectOffsetOriginal.X * scrollScale;
        //        selectOffset.Y = selectOffsetOriginal.Y * scrollScale;

        //        // przeniesienie selectRect do poczatku bitmapRect + selectOffset
        //        selectRect.Left += bitmapRect.Left + selectOffset.X;
        //        selectRect.Top += bitmapRect.Top + selectOffset.Y;
        //        selectRect.Right += bitmapRect.Left + selectOffset.X;
        //        selectRect.Bottom += bitmapRect.Top + selectOffset.Y;
        //    }

        //    public bool PointInRectangle(SKPoint M, SKPoint[] r)
        //    {
        //        //var A = new SKPoint(r.Left, r.Top);
        //        //var B = new SKPoint(r.Right, r.Top);
        //        //var C = new SKPoint(r.Left, r.Bottom);

        //        var A = r[0];
        //        var B = r[1];
        //        var C = r[2];

        //        var AB = MakeVector(A, B);
        //        var AM = MakeVector(A, M);
        //        var BC = MakeVector(B, C);
        //        var BM = MakeVector(B, M);

        //        var dotABAM = DotProduct(AB, AM);
        //        var dotABAB = DotProduct(AB, AB);
        //        var dotBCBM = DotProduct(BC, BM);
        //        var dotBCBC = DotProduct(BC, BC);

        //        return 0 <= dotABAM && dotABAM <= dotABAB && 0 <= dotBCBM && dotBCBM <= dotBCBC;
        //    }

        //    public SKPoint MakeVector(SKPoint p1, SKPoint p2)
        //    {
        //        return new SKPoint(p2.X - p1.X, p2.Y - p1.Y);
        //    }

        //    public float DotProduct(SKPoint u, SKPoint v)
        //    {
        //        return u.X * v.X + u.Y * v.Y;
        //    }

        //    public void ResetTilt()
        //    {
        //        tiltAngle = 0;
        //        tilting = false;
        //        skiaView.Invalidate();
        //    }

        //    public void TiltImage()
        //    {
        //        //sKBitmap = TiltBitmap(degrees);
        //        //AddBitmap(sKBitmap);
        //        //ResetPanOffset();
        //        //ResetScale();

        //        //tiltAngle += degrees;
        //        //tiltAngle = degrees;
        //        tilting = true;
        //        makeSelectRect = true;
        //        ResetPanOffset();
        //        skiaView.Invalidate();
        //    }

        //    public void ShowSKMatrix(SKMatrix result, string title)
        //    {
        //        string str = $"{title}: ";
        //        foreach (var item in result.Values)
        //        {
        //            str += $"{item}, ";
        //        }
        //        //Console.WriteLine(str);
        //    }

        //    public void FlipImage(bool isHorizontal)
        //    {
        //        //sKBitmap = FlipBitmap(isHorizontal);
        //        AddBitmap(FlipBitmap(isHorizontal));
        //        if (selectRect != SKRect.Empty)
        //            FlipRect2(isHorizontal);
        //        ResetPanOffset();
        //        //ResetScale();

        //        skiaView.Invalidate();
        //    }

        //    public SKBitmap FlipBitmap(bool isHorizontal)
        //    {
        //        SKBitmap flippedBitmap = new SKBitmap(sKBitmap.Width, sKBitmap.Height);

        //        using (SKCanvas canvas = new SKCanvas(flippedBitmap))
        //        {
        //            canvas.Clear();
        //            if (isHorizontal)
        //            {
        //                canvas.Translate(sKBitmap.Width, 0);
        //                canvas.Scale(-1, 1);
        //            }
        //            else
        //            {
        //                canvas.Translate(0, sKBitmap.Height);
        //                canvas.Scale(1, -1);
        //            }
        //            canvas.DrawBitmap(sKBitmap, new SKPoint(0, 0));
        //        }

        //        return flippedBitmap;
        //    }

        //    // using skmatrix transforms
        //    public void FlipRect2(bool isHorizontal)
        //    {
        //        SKMatrix translate1, translate2, translate3, scale;

        //        //selectRect.Left -= bitmapRect.Left;
        //        //selectRect.Top -= bitmapRect.Top;
        //        //selectRect.Right -= bitmapRect.Left;
        //        //selectRect.Bottom -= bitmapRect.Top;
        //        translate1 = SKMatrix.CreateTranslation(-bitmapRect.Left, -bitmapRect.Top);

        //        var result = SKMatrix.CreateIdentity();
        //        if (isHorizontal)
        //        {
        //            translate2 = SKMatrix.CreateTranslation(bitmapRect.Width, 0);
        //            scale = SKMatrix.CreateScale(-1, 1);
        //        }
        //        else
        //        {
        //            translate2 = SKMatrix.CreateTranslation(0, bitmapRect.Height);
        //            scale = SKMatrix.CreateScale(1, -1);
        //        }

        //        //selectRect.Left += bitmapRect.Left;
        //        //selectRect.Top += bitmapRect.Top;
        //        //selectRect.Right += bitmapRect.Left;
        //        //selectRect.Bottom += bitmapRect.Top;
        //        translate3 = SKMatrix.CreateTranslation(bitmapRect.Left, bitmapRect.Top);

        //        SKMatrix.PreConcat(ref result, translate3);
        //        SKMatrix.PreConcat(ref result, translate2);
        //        SKMatrix.PreConcat(ref result, scale);
        //        SKMatrix.PreConcat(ref result, translate1);
        //        selectRect = result.MapRect(selectRect);

        //        SetUpNewSelectRect();
        //    }

        //    public void RotateImage()
        //    {
        //        if (selectRect != SKRect.Empty)
        //            RotateRect3();
        //        //sKBitmap = RotateBitmap();
        //        AddBitmap(RotateBitmap());
        //        ResetPanOffset();
        //        //ResetScale();

        //        skiaView.Invalidate();
        //    }

        //    public SKBitmap RotateBitmap()
        //    {
        //        SKBitmap rotatedBitmap = new SKBitmap(sKBitmap.Height, sKBitmap.Width);
        //        using (SKCanvas canvas = new SKCanvas(rotatedBitmap))
        //        {
        //            canvas.Clear();
        //            canvas.Translate(sKBitmap.Height, 0);
        //            canvas.RotateDegrees(90);
        //            canvas.DrawBitmap(sKBitmap, new SKPoint(0, 0));
        //        }

        //        return rotatedBitmap;
        //    }

        //    // both RotateRect()s dont work properly for clicking rotate fast 
        //    // skmatrix transforms
        //    public void RotateRect3()
        //    {
        //        SKMatrix rotate, scale, translate1, translate2;
        //        SKMatrix result = SKMatrix.CreateIdentity();
        //        //SKMatrix translate = SKMatrix.CreateTranslation(-selectOffset.X, -selectOffset.Y);
        //        translate1 = SKMatrix.CreateTranslation(-bitmapRect.Left, -bitmapRect.Top);
        //        float bitmaptoViewRatio = (float)sKBitmap.Width / bitmapRect.Width;
        //        scale = SKMatrix.CreateScale(bitmaptoViewRatio, bitmaptoViewRatio);
        //        rotate = SKMatrix.CreateRotationDegrees(90);
        //        translate2 = SKMatrix.CreateTranslation(sKBitmap.Height, 0);

        //        SKMatrix.PreConcat(ref result, translate2);
        //        SKMatrix.PreConcat(ref result, rotate);
        //        SKMatrix.PreConcat(ref result, scale);
        //        SKMatrix.PreConcat(ref result, translate1);
        //        selectRect = result.MapRect(selectRect);

        //        OrderSelectRect();
        //        rotateSelect = true;
        //    }

        //    [JSInvokable]
        //    public void OnKeyPressedJS(bool eCtrlKey, bool eShiftKey, string eCode)
        //    {
        //        //Console.WriteLine($"c#: e.CtrlKey: {eCtrlKey}, e.ShiftKey: {eShiftKey}, e.Key: {eCode}");
        //        if (eCode == "KeyZ")
        //        {
        //            if (eCtrlKey)
        //            {
        //                if (eShiftKey)
        //                {
        //                    currentIndex++;
        //                }
        //                else
        //                {
        //                    currentIndex--;
        //                }
        //                currentIndex = Math.Max(0, currentIndex);
        //                currentIndex = Math.Min(Math.Max(0, lastBitmaps.Count - 1), currentIndex);

        //                sKBitmap = lastBitmaps[currentIndex];
        //                //Console.WriteLine($"currentIndex: {currentIndex}");
        //                skiaView.Invalidate();
        //            }
        //        }
        //        if (eCode == "KeyS")
        //        {
        //            SaveImage();
        //        }
        //    }

        //    private void AddBitmap(SKBitmap bitmap)
        //    {
        //        sKBitmap = bitmap;
        //        //int imax = 5;
        //        if (currentIndex < lastBitmaps.Count - 1)
        //        {
        //            lastBitmaps.RemoveRange(currentIndex + 1, lastBitmaps.Count - currentIndex - 1);
        //            lastBitmaps.Add(bitmap);
        //            currentIndex++;
        //        }
        //        else
        //        {
        //            if (lastBitmaps.Count < imax)
        //            {
        //                lastBitmaps.Add(bitmap);
        //                currentIndex++;
        //            }
        //            else
        //            {
        //                lastBitmaps = lastBitmaps.GetRange(1, lastBitmaps.Count - 1);
        //                lastBitmaps.Add(bitmap);
        //            }
        //        }
        //    }

        //    private void OrderSelectRect()
        //    {
        //        if (selectRect.Left > selectRect.Right)
        //        {
        //            float tLeft = selectRect.Left;
        //            selectRect.Left = selectRect.Right;
        //            selectRect.Right = tLeft;
        //        }

        //        if (selectRect.Top > selectRect.Bottom)
        //        {
        //            float tTop = selectRect.Top;
        //            selectRect.Top = selectRect.Bottom;
        //            selectRect.Bottom = tTop;
        //        }
        //    }

        //    // using skmatrix transform + works + tilt cropping works
        //    private void CropImage()
        //    {
        //        if (selectRect != SKRect.Empty)
        //        {
        //            SKMatrix scale, translate, result;
        //            float bitmapToViewRatio = (float)sKBitmap.Width / bitmapRect.Width;

        //            // order selection start and end
        //            OrderSelectRect();

        //            // odjac poczatek bitmapy od selectRect
        //            translate = SKMatrix.CreateTranslation(-bitmapRect.Left, -bitmapRect.Top);
        //            // przeskalowac selectRect o bitmapToViewRatio
        //            scale = SKMatrix.CreateScale(bitmapToViewRatio, bitmapToViewRatio);

        //            SKRect cropRect = SKRect.Empty;
        //            result = SKMatrix.CreateIdentity();
        //            SKMatrix.PreConcat(ref result, scale);
        //            SKMatrix.PreConcat(ref result, translate);
        //            cropRect = result.MapRect(selectRect);

        //            float movX = -new List<float>() { cropRect.Left, cropRect.Right, 0 }.Min();
        //            float movY = -new List<float>() { cropRect.Top, cropRect.Bottom, 0 }.Min();
        //            result = SKMatrix.CreateIdentity();
        //            translate = SKMatrix.CreateTranslation(movX, movY);
        //            SKMatrix.PreConcat(ref result, translate);
        //            cropRect = result.MapRect(cropRect);

        //            //Console.WriteLine($"cR: ({cropRect.Left}, {cropRect.Top}), ({cropRect.Right}, {cropRect.Bottom})");
        //            //Console.WriteLine($"movX: {movX}, movY: {movY}");

        //            SKRect dest = new SKRect(0, 0, cropRect.Width, cropRect.Height);
        //            SKBitmap croppedBitmap = new SKBitmap((int)cropRect.Width, (int)cropRect.Height);

        //            using (SKCanvas canvas = new SKCanvas(croppedBitmap))
        //            {
        //                if (tilting)
        //                {
        //                    // revise size
        //                    int rBSize = 2 * Math.Max(sKBitmap.Width, sKBitmap.Height);
        //                    SKBitmap rotatedSKBitmap = new SKBitmap(rBSize, rBSize);
        //                    using (SKCanvas canvas2 = new SKCanvas(rotatedSKBitmap))
        //                    {
        //                        canvas2.Translate(movX, movY);
        //                        canvas2.RotateDegrees(tiltAngle, sKBitmap.Width / 2, sKBitmap.Height / 2);
        //                        canvas2.DrawBitmap(sKBitmap, new SKPoint(0, 0));
        //                        canvas2.RotateDegrees(-tiltAngle, sKBitmap.Width / 2, sKBitmap.Height / 2);
        //                    }
        //                    canvas.DrawBitmap(rotatedSKBitmap, cropRect, dest);
        //                    rotatedSKBitmap.Dispose();
        //                }
        //                else
        //                    canvas.DrawBitmap(sKBitmap, cropRect, dest);
        //            }

        //            AddBitmap(croppedBitmap);
        //            selectRect = SKRect.Empty;
        //            ResetPanOffset();
        //            if (scrollScale > 1)
        //                ResetScale();
        //            if (tilting)
        //                ResetTilt();
        //            skiaView.Invalidate();
        //        }
        //        else
        //            Console.WriteLine("No selection error message");
        //    }

        //    private void ResetPanOffset()
        //    {
        //        panOffset = new SKPoint(0, 0);
        //        scrollScaleChanged = true;
        //        skiaView.Invalidate();
        //    }

        //    private void ResetScale()
        //    {
        //        scrollScale = 1;
        //        scrollScaleChanged = true;
        //        skiaView.Invalidate();
        //    }

        //    private float Distance(float x1, float y1, float x2, float y2)
        //    {
        //        return (float)Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        //    }

        //    private (float scaledOffsetX, float scaledOffsetY) ScaleCoordinates(float eOffsetX, float eOffsetY)
        //    {
        //        return ((float)eOffsetX * windowDPR, (float)eOffsetY * windowDPR);
        //    }

        //    private void ReverseSelection()
        //    {
        //        float left = selectRect.Left;
        //        float top = selectRect.Top;

        //        selectRect.Left = selectRect.Right;
        //        selectRect.Right = left;
        //        selectRect.Top = selectRect.Bottom;
        //        selectRect.Bottom = top;
        //        // seemed to work without se1, except for: create sR > edit sr(L,T) > zoom in/out
        //        //s1
        //        //selectRectOriginal.Left = selectRectOriginal.Right;
        //        //selectRectOriginal.Right = left;
        //        //selectRectOriginal.Top = selectRectOriginal.Bottom;
        //        //selectRectOriginal.Bottom = top;
        //        //e1
        //    }

        //    private void SetSelectionOffset()
        //    {
        //        selectOffset.X = selectRect.Left - bitmapRect.Left;
        //        selectOffset.Y = selectRect.Top - bitmapRect.Top;
        //        //Console.WriteLine($"b:({bitmapRect.Left}, {bitmapRect.Top})\n"+
        //        //$"s:({selectRect.Left}, {selectRect.Top})\n"+ 
        //        //$"o: ({selectOffset.X}, {selectOffset.Y}))");
        //    }

        //    private void ModifySelectStart(SKPoint S)
        //    {
        //        SKPoint d = S - new SKPoint(selectRect.Right, selectRect.Bottom);
        //        SKPoint point = S + d;
        //        selectRect.Left = point.X;
        //        selectRect.Top = point.Y;
        //    }

        //    private SKRect MakeSelectSymmetric(SKPoint S, SKPoint start)
        //    {
        //        SKPoint d = S - start;
        //        SKPoint end = S + d;
        //        return new SKRect(start.X, start.Y, end.X, end.Y);
        //    }

        //    private bool IsPointOnALine(SKPoint A, SKPoint B, SKPoint P)
        //    {
        //        float eps = 0.01f;
        //        float AP = SKPoint.Distance(A, P);
        //        float PB = SKPoint.Distance(P, B);
        //        float AB = SKPoint.Distance(A, B);

        //        //Console.WriteLine($"IsPointOnALine: {AB}, {AP + PB}, {AB == AP + PB}");
        //        //Console.WriteLine($"IsPointOnALine: {AB}, {AP + PB}, {Math.Abs(AB - (AP + PB)) < eps}");
        //        //return AB == AP + PB;
        //        return Math.Abs(AB - (AP + PB)) < eps;
        //    }

        //    // SetSelectionStart3 + works
        //    private void SetSelectionStart(double offsetX, double offsetY)
        //    {
        //        if (tilting)
        //        {
        //            SetSelectionEnd(offsetX, offsetY);
        //        }
        //        else
        //        {
        //            selectRect.Left = (float)Math.Max(bitmapRect.Left, offsetX);
        //            selectRect.Top = (float)Math.Max(bitmapRect.Top, offsetY);

        //            selectRect.Left = (float)Math.Min(bitmapRect.Right, selectRect.Left);
        //            selectRect.Top = (float)Math.Min(bitmapRect.Bottom, selectRect.Top);
        //        }
        //    }

        //    private SKPoint CalcSelectRect(float offsetX, float offsetY)
        //    {
        //        SKPoint result = new SKPoint();
        //        P = new SKPoint[] { };

        //        SKPoint A = new SKPoint(offsetX, offsetY);
        //        SKPoint S = new SKPoint(infoMiddleX, infoMiddleY);

        //        float b = A.Y - A.X * (A.Y - S.Y) / (A.X - S.X);
        //        SKPoint B = new SKPoint(0, b);
        //        //SKPoint B2 = new SKPoint(2 * S.X, 0);
        //        SKPoint B2 = new SKPoint(2 * S.X, b);

        //        //SKPath BSB2 = new SKPath();
        //        BSB2 = new SKPath();
        //        BSB2.MoveTo(B);
        //        BSB2.LineTo(S);
        //        BSB2.LineTo(B2);

        //        //SKPath tiltedBPath = new SKPath();
        //        tiltedBPath = new SKPath();
        //        tiltedBPath.MoveTo(tiltedBitmapCorners[0]);
        //        tiltedBPath.LineTo(tiltedBitmapCorners[1]);
        //        tiltedBPath.LineTo(tiltedBitmapCorners[2]);
        //        tiltedBPath.LineTo(tiltedBitmapCorners[3]);
        //        //tiltedBPath.LineTo(tiltedBitmapCorners[0]);
        //        tiltedBPath.Close();

        //        //SKPath intersection = BSB2.Op(tiltedBPath, SKPathOp.Intersect);
        //        SKPath intersection = tiltedBPath.Op(BSB2, SKPathOp.Intersect);
        //        if (intersection != null)
        //            P = intersection.Points;

        //        //s3
        //        var pList = new List<(float dist, int index)>();
        //        if (P != null)
        //        {
        //            int i = 0;
        //            foreach (var point in P)
        //            {
        //                // if point is between BS or B2S add to pList
        //                if (IsPointOnALine(S, B, point) || IsPointOnALine(S, B2, point))
        //                {
        //                    float d = SKPoint.Distance(S, point);
        //                    if (d != 0)
        //                        pList.Add((d, i));
        //                }
        //                i++;
        //            }
        //            //pList.Sort();

        //            if (pList.Count > 0)
        //                result = P[pList.Min().index];
        //        }
        //        //e3
        //        return result;
        //    }

        //    // SetSelectionEnd4 + works latest 3
        //    private void SetSelectionEnd(double offsetX, double offsetY)
        //    {
        //        if (tilting)
        //        {
        //            SKPoint start = CalcSelectRect((float)offsetX, (float)offsetY);
        //            SKRect newSelect = MakeSelectSymmetric(new SKPoint(infoMiddleX, infoMiddleY), start);
        //            if (newSelect != SKRect.Empty)
        //            {
        //                selectRect = newSelect;
        //                OrderSelectRect();
        //                SetSelectionOffset();
        //                //Console.WriteLine("PointInRectangle End: true");
        //            }
        //            //else { Console.WriteLine("PointInRectangle End: false"); }
        //        }
        //        else
        //        {
        //            selectRect.Right = (float)Math.Min(bitmapRect.Right, offsetX);
        //            selectRect.Bottom = (float)Math.Min(bitmapRect.Bottom, offsetY);

        //            selectRect.Right = (float)Math.Max(bitmapRect.Left, selectRect.Right);
        //            selectRect.Bottom = (float)Math.Max(bitmapRect.Top, selectRect.Bottom);
        //        }
        //    }

        //    private void SetSelectOriginal()
        //    {
        //        selectRectOriginal.Left = selectRect.Left / scrollScale;
        //        selectRectOriginal.Top = selectRect.Top / scrollScale;
        //        selectRectOriginal.Right = selectRect.Right / scrollScale;
        //        selectRectOriginal.Bottom = selectRect.Bottom / scrollScale;

        //        selectOffsetOriginal.X = selectOffset.X / scrollScale;
        //        selectOffsetOriginal.Y = selectOffset.Y / scrollScale;
        //    }

        //    [JSInvokable]
        //    public void SetWindowDPR(float pr)
        //    {
        //        windowDPR = pr;
        //        //Console.WriteLine("SetWindowDPR");
        //    }

        //    // LoadImageToEdit2
        //    public async Task LoadImageToEdit()
        //    {
        //        ImageName = await LSManager.GetImageName();
        //        ImageId = await LSManager.GetImageId();
        //        if (ImageId != null)
        //        {
        //            imageResized2 = await IDbManager.FetchImageResized2((int)ImageId);
        //            if (imageResized2 != null)
        //            {
        //                //sKBitmap = SKBitmap.Decode(imageResized.ImageArray);
        //                //testSrc = await ImageProc.GetImageURL(imageResized.ImageArray);
        //            }

        //            // get original image
        //            imageOriginal2 = await IDbManager.FetchImageOriginal2((int)ImageId);
        //            if (imageOriginal2 != null)
        //            {
        //                AddBitmap(SKBitmap.Decode(imageOriginal2.Array));
        //                //testSrc2 = await ImageProc.GetImageURL(imageData.ImageArray);
        //            }
        //        }
        //    }
    }
}