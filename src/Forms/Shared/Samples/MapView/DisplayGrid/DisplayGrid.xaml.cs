// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using Esri.ArcGISRuntime.Geometry;
using Xamarin.Forms;
using Colors = System.Drawing.Color;

namespace ArcGISRuntime.Samples.DisplayGrid
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display a grid",
        "MapView",
        "This sample demonstrates how to display and work with coordinate grid systems such as Latitude/Longitude, MGRS, UTM and USNG on a map view. This includes toggling labels visibility, changing the color of the grid lines, and changing the color of the grid labels.",
        "Choose the grid settings and then tap 'Apply settings' to see them applied.")]
    public partial class DisplayGrid : ContentPage
    {
        public DisplayGrid()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private void Initialize()
        {
            // Set up the map view with a basemap.
            MyMapView.Map = new Map(Basemap.CreateImageryWithLabels());

            // Configure the UI options.
            gridTypePicker.ItemsSource = new[] {"LatLong", "MGRS", "UTM", "USNG"};
            string[] colorItemsSource = {"Red", "Green", "Blue", "White", "Purple"};
            gridColorPicker.ItemsSource = colorItemsSource;
            labelColorPicker.ItemsSource = colorItemsSource;
            haloColorPicker.ItemsSource = colorItemsSource;
            labelPositionPicker.ItemsSource = Enum.GetNames(typeof(GridLabelPosition));
            labelFormatPicker.ItemsSource = Enum.GetNames(typeof(LatitudeLongitudeGridLabelFormat));
            foreach (Picker combo in new[]
                {gridTypePicker, gridColorPicker, labelColorPicker, labelPositionPicker, labelFormatPicker})
            {
                combo.SelectedIndex = 0;
            }

            // Update the halo color to have a good default.
            haloColorPicker.SelectedIndex = 3;

            // Handle grid type changes so that the format option can be disabled for non-latlong grids.
            gridTypePicker.SelectedIndexChanged += (o, e) =>
            {
                labelFormatPicker.IsEnabled = gridTypePicker.SelectedItem.ToString() == "LatLong";
            };

            // Subscribe to the button click event.
            applySettingsButton.Clicked += ApplySettingsButton_Clicked;

            // Enable the action button.
            applySettingsButton.IsEnabled = true;

            // Zoom to a default scale that will show the grid labels if they are enabled.
            MyMapView.SetViewpointCenterAsync(
                new MapPoint(-7702852.905619, 6217972.345771, SpatialReferences.WebMercator), 23227);

            // Apply default settings.
            ApplySettingsButton_Clicked(this, null);
        }

        private void ApplySettingsButton_Clicked(object sender, EventArgs e)
        {
            Esri.ArcGISRuntime.UI.Grid grid;

            // First, update the grid based on the type selected.
            switch (gridTypePicker.SelectedItem.ToString())
            {
                case "LatLong":
                    grid = new LatitudeLongitudeGrid();
                    // Apply the label format setting.
                    string selectedFormatString = labelFormatPicker.SelectedItem.ToString();
                    ((LatitudeLongitudeGrid) grid).LabelFormat =
                        (LatitudeLongitudeGridLabelFormat) Enum.Parse(typeof(LatitudeLongitudeGridLabelFormat),
                            selectedFormatString);
                    break;

                case "MGRS":
                    grid = new MgrsGrid();
                    break;

                case "UTM":
                    grid = new UtmGrid();
                    break;
                case "USNG":
                default:
                    grid = new UsngGrid();
                    break;
            }

            // Next, apply the label visibility setting.
            grid.IsLabelVisible = labelVisibilitySwitch.IsToggled;

            // Next, apply the grid visibility setting.
            grid.IsVisible = gridVisibilitySwitch.IsToggled;

            // Next, apply the grid color and label color settings for each zoom level.
            for (long level = 0; level < grid.LevelCount; level++)
            {
                // Set the line symbol.
                Symbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid,
                    Colors.FromName(gridColorPicker.SelectedItem.ToString()), 2);
                grid.SetLineSymbol(level, lineSymbol);

                // Set the text symbol.
                Symbol textSymbol = new TextSymbol
                {
                    Color = Colors.FromName(labelColorPicker.SelectedItem.ToString()),
                    OutlineColor = Colors.FromName(haloColorPicker.SelectedItem.ToString()),
                    Size = 16,
                    HaloColor = Colors.FromName(haloColorPicker.SelectedItem.ToString()),
                    HaloWidth = 3
                };
                grid.SetTextSymbol(level, textSymbol);
            }

            // Next, apply the label position setting.
            grid.LabelPosition =
                (GridLabelPosition) Enum.Parse(typeof(GridLabelPosition), labelPositionPicker.SelectedItem.ToString());

            // Apply the updated grid.
            MyMapView.Grid = grid;
        }
    }
}