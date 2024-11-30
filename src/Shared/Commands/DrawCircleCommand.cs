using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace MyApp.Commands;

public class DrawCircleCommand
{
#if DEBUG
    [CommandMethod("DrawCircle")]
#endif
    public void DrawCircle()
    {
        // Get the current document and database
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Database db = doc.Database;
        Editor editor = doc.Editor;

        // Prompt the user for the center point of the circle
        PromptPointOptions promptPointOptions = new PromptPointOptions("\nSelect center point of the circle: ");
        PromptPointResult promptPointResult = editor.GetPoint(promptPointOptions);

        // Check if the user canceled the operation
        if (promptPointResult.Status != PromptStatus.OK)
        {
            editor.WriteMessage("\nOperation canceled.");
            return;
        }

        // Get the selected center point
        Point3d centerPoint = promptPointResult.Value;

        // Fixed radius of 5 units
        double radius = 5.0;

        // Start a transaction
        using (Transaction trans = db.TransactionManager.StartTransaction())
        {
            // Open the Block Table for read
            BlockTable blockTable = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);

            // Open the Block Table Record (Model Space) for write
            BlockTableRecord blockTableRecord = (BlockTableRecord)trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            // Create the circle with the selected center point and fixed radius
            Circle circle = new Circle(centerPoint, Vector3d.ZAxis, radius);

            // Add the circle to the Block Table Record (Model Space)
            blockTableRecord.AppendEntity(circle);
            trans.AddNewlyCreatedDBObject(circle, true);

            // Commit the transaction
            trans.Commit();
        }

        // Print a message to the AutoCAD command line
        editor.WriteMessage($"\nCircle with a radius of {radius} units created at {centerPoint}.");
    }
}