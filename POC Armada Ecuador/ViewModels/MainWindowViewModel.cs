using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;

using Prism.Commands;
using Prism.Mvvm;

namespace POC_Armada_Ecuador.ViewModels {
  public class MainWindowViewModel : BindableBase {
    private string _title = "Prueba de Concepto";
    private bool _areSymbolScaled = false;
    private Map _map;
    private MapPoint _ship;
    private Point _screenShip;
    private GraphicsOverlayCollection _graphicsOverlays;

    public string Title {
      get => _title;
      set => SetProperty(ref _title, value);
    }

    public Map Map {
      get => _map;
      set => SetProperty(ref _map, value);
    }

    public GraphicsOverlayCollection GraphicsOverlays {
      get => _graphicsOverlays;
      set => SetProperty(ref _graphicsOverlays, value);
    }

    public bool AreSymbolScaled {
      get => _areSymbolScaled;
      set => SetProperty(ref _areSymbolScaled, value);
    }

    public MapPoint Ship {
      get => _ship;
      set => SetProperty(ref _ship, value);
    }

    public Point ScreenShip {
      get => _screenShip;
      set => SetProperty(ref _screenShip, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public DelegateCommand<MapView> UpdateScreenShipCommand { get; private set; }

    /// <summary>
    /// /
    /// </summary>
    public DelegateCommand<MapView> LoadMapViewCommand { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public DelegateCommand<Canvas> LoadWindowCommand { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public DelegateCommand AddEllipseCommand { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public DelegateCommand AddCircleCommand { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    private Canvas ManeuveringBoardCanvas { get; set; }

    /// <summary>
    /// 
    /// </summary>
    private MapView MapView { get; set; }

    public MainWindowViewModel() {
      UpdateScreenShipCommand = new DelegateCommand<MapView>(mapView => {
        ScreenShip = mapView.LocationToScreen(Ship);
        UpdateCanvas();
      });

      LoadWindowCommand = new DelegateCommand<Canvas>(canvas => ManeuveringBoardCanvas = canvas);

      LoadMapViewCommand = new DelegateCommand<MapView>(mapView => {
        MapView = mapView;
        ScreenShip = mapView.LocationToScreen(Ship);
        UpdateCanvas();
      });

      AddEllipseCommand = new DelegateCommand(async () => {
        if(MapView != null) {
          var geometry = await MapView.SketchEditor.StartAsync(SketchCreationMode.Ellipse, false);
          AddGraphic(geometry, SketchCreationMode.Ellipse);
        }
      });
      AddCircleCommand = new DelegateCommand(async () => {
        if(MapView != null) {
          var geometry = await MapView.SketchEditor.StartAsync(SketchCreationMode.Circle, false);
          AddGraphic(geometry, SketchCreationMode.Circle);
        }
      });

      SetMap();
      SetGraphicsOverlays();
    }

    private void AddGraphic(Esri.ArcGISRuntime.Geometry.Geometry geometry, SketchCreationMode mode) {
      var ceo = GraphicsOverlays.FirstOrDefault(go => go.Id == "CirclesElipsesOverlay");
      if(ceo != null) {
        var g = new Graphic() {
          Geometry = geometry,
          Symbol = MapView.SketchEditor.Style.FeedbackFillSymbol
        };
        ceo.Graphics.Add(g);
        var center = geometry.Extent.GetCenter();
        var x = center.X;
        var y = center.Y;

        var texto = string.Empty;
        if(mode == SketchCreationMode.Circle) {
          var r = geometry.Extent.Width / 2;

          var area = Math.Abs(geometry.Area());
          var r0 = Math.Sqrt(area / Math.PI);
          Console.WriteLine(r0);

          texto = $"x = {x}\ny = {y}\nr = {r}";
        }
        else {
          var r1 = geometry.Extent.Width / 2;
          var r2 = geometry.Extent.Height / 2;
          texto = $"x = {x}\ny = {y}\nr1 = {r1}\nr2 = {r2}";
        }

        var t = new Graphic() {
          Geometry = center,
          Symbol = new TextSymbol() {
            Text = texto,
            Size = 12,
            VerticalAlignment = Esri.ArcGISRuntime.Symbology.VerticalAlignment.Middle,
            HorizontalAlignment = Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Center
          }
        };
        ceo.Graphics.Add(t);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateCanvas() {
      var offset = VisualTreeHelper.GetOffset(ManeuveringBoardCanvas);
      var delta = new Point(ScreenShip.X - offset.X - (0.5 * ManeuveringBoardCanvas.Width), ScreenShip.Y - offset.Y - (0.5 * ManeuveringBoardCanvas.Height));
      var transform = ManeuveringBoardCanvas.RenderTransform;
      (transform as TranslateTransform).X = delta.X;
      (transform as TranslateTransform).Y = delta.Y;

      var mbo = GraphicsOverlays.FirstOrDefault(go => go.Id == "ManeuveringBoard");
      if(mbo != null) {
        var text = mbo.Graphics.FirstOrDefault(g => g.Symbol.GetType() == typeof(TextSymbol));
        var x = (int)ScreenShip.X;
        var y = (int)ScreenShip.Y;
        if(text != null) {
          (text.Symbol as TextSymbol).Text = $"{x},{y}";
        }
      }
    }

    private void SetMap() {
      Map = new Map(BasemapStyle.ArcGISNavigationNight);
      Map.Loaded += (o, e) => {
        var center = new MapPoint(-80.9177882103182, -2.606651537154965, SpatialReferences.Wgs84);
        Map.InitialViewpoint = new Viewpoint(center, scale: 1000000);
      };
    }

    private void SetGraphicsOverlays() {
      GraphicsOverlays = new GraphicsOverlayCollection();

      var circlesOverlay = new GraphicsOverlay {
        Id = "CirclesElipsesOverlay",
        ScaleSymbols = true
      };
      GraphicsOverlays.Add(circlesOverlay);

      var maneuveringBoardOverlay = new GraphicsOverlay {
        Id = "ManeuveringBoard",
        ScaleSymbols = true
      };
      GraphicsOverlays.Add(maneuveringBoardOverlay);

      Ship = new MapPoint(-80.9177882103182, -2.606651537154965, SpatialReferences.Wgs84);
      var center = new Graphic() {
        Geometry = Ship,
        Symbol = new SimpleMarkerSymbol() {
          Color = System.Drawing.Color.Black,
          Style = SimpleMarkerSymbolStyle.Square
        }
      };

      Ship = new MapPoint(-80.9177882103182, -2.606651537154965, SpatialReferences.Wgs84);
      var text = new Graphic() {
        Geometry = Ship,
        Symbol = new TextSymbol() {
          Color = System.Drawing.Color.White,
          Text = "NA",
          Size = 20,
          OffsetX = 50,
          HorizontalAlignment = Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Left,
        }
      };

      var fillSymbol = new SimpleFillSymbol() {
        Outline = new SimpleLineSymbol() {
          Color = System.Drawing.Color.Black,
          Style = SimpleLineSymbolStyle.Solid,
          Width = 2
        },
        Style = SimpleFillSymbolStyle.Null,
        Color = System.Drawing.Color.Red
      };
      var multilayerSymbol = fillSymbol.ToMultilayerSymbol();
      var circle = new Graphic() {
        Geometry = Circle(new MapPoint(-80.9177882103182, -2.606651537154965, SpatialReferences.Wgs84), 1),
        Symbol = multilayerSymbol
      };

      maneuveringBoardOverlay.Graphics.Add(center);
      maneuveringBoardOverlay.Graphics.Add(text);
      maneuveringBoardOverlay.Graphics.Add(circle);
      Console.WriteLine(circle.ToString());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="center"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    private Polygon Circle(MapPoint center, double radius) {
      var segments = new List<Segment>();
      var x = center.X + (radius * Math.Cos(0));
      var y = center.Y + (radius * Math.Sin(0));
      var startPoint = new MapPoint(x, y, center.SpatialReference);
      var index = 1;

      while(index < 37) {
        var rad = index * Math.PI / 18;
        x = center.X + (radius * Math.Cos(rad));
        y = center.Y + (radius * Math.Sin(rad));
        var endPoint = new MapPoint(x, y, center.SpatialReference);
        segments.Add(new EllipticArcSegment(startPoint, endPoint, rad, true, true, radius, 1, center.SpatialReference));
        startPoint = endPoint;
        index++;
      }
      return new Polygon(segments);
    }
  }
}
