using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using MyApp.Core;
using System;

namespace MyApp.Services;

public class SelectService
{
    public (int SelectedCount, double TotalLength, string Message) SelectObjects(SelectOptions selectOptions)
    {
        try
        {
            // Get the current document and editor
            var document = Application.DocumentManager.MdiActiveDocument;
            var editor = document.Editor;

            // Define a selection filter for specific object types
            var filter = new SelectionFilter(new[]
            {
                new TypedValue((int)DxfCode.Start,selectOptions.ToString())
            });

            // Prompt the user to select objects
            var result = editor.GetSelection(filter);

            if (result.Status == PromptStatus.OK)
            {
                var selectedObjects = result.Value;
                int selectedCount = selectedObjects.Count;
                double totalLength = 0.0;

                using (var transaction = document.TransactionManager.StartTransaction())
                {
                    foreach (var id in selectedObjects.GetObjectIds())
                    {
                        var entity = transaction.GetObject(id, OpenMode.ForRead) as Entity;

                        if (entity is Curve curve)
                        {
                            totalLength += curve.GetDistanceAtParameter(curve.EndParam) - curve.GetDistanceAtParameter(curve.StartParam);
                        }
                    }

                    transaction.Commit();
                }

                return (selectedCount, totalLength, $"Selected {selectedCount} objects. Total Length: {totalLength:F2}");
            }
            else
            {
                return (0, 0.0, "No objects selected.");
            }
        }
        catch (Exception ex)
        {
            return (0, 0.0, $"Error: {ex.Message}");
        }
    }
}
