using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace RevitFinalPlugin
{
    [TransactionAttribute(TransactionMode.Manual)]//
    public class CreateTags : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;//получение ссылки на Юай документ
            Document doc = uiDoc.Document;//получение ссылки на экземпляр класса документ, со ссылкой на бд открытого документа

            //Level level = new FilteredElementCollector(doc)
            //    .OfClass(typeof(Level))
            //    .OfType<Level>()
            //    .FirstOrDefault();

            //фильтр уровней
            List<Level> listlevel = new FilteredElementCollector(doc)
                     .OfClass(typeof(Level))
                     .OfType<Level>()
                     .ToList();


            Transaction transaction = new Transaction(doc, "Создание и маркировка");
            transaction.Start();
            //List<Room> rooms = CreateRoom(doc, level);
            //метод для создания помещений
            List<Room> rooms = CreateRoom(doc, listlevel);
            //смена маркировки с добавлением уровня
            ChangeID(doc);
            //проявление маркировки для промаркированных помещений
            CreateTag(doc);
            transaction.Commit();
            return Result.Succeeded;
        }
        //public List<Room> CreateRoom(Document document, Level level)
        //{
        //    PlanTopology pt = document.get_PlanTopology(level);
        //    List<Room> rooms = new List<Room>();
        //    foreach (PlanCircuit pc in pt.Circuits)
        //    {
        //        if (!pc.IsRoomLocated)
        //        {
        //            Room r = document.Create.NewRoom(null, pc);
        //            rooms.Add(r);
        //        }
        //    }
        //    return rooms;
        //}

        //метод для создания комнаты
        public List<Room> CreateRoom(Document document, List<Level> levels)
        {
            List<Room> rooms = new List<Room>();
            foreach (Level level in levels)
            {
                PlanTopology pt = document.get_PlanTopology(level);

                foreach (PlanCircuit pc in pt.Circuits)
                {
                    if (!pc.IsRoomLocated)
                    {
                        Room r = document.Create.NewRoom(null, pc);
                        rooms.Add(r);
                    }
                }
            }
            return rooms;
        }
        //метод изменения номера комнаты
        public void ChangeID(Document document)
        {
            var rooms = new FilteredElementCollector(document)
               .OfCategory(BuiltInCategory.OST_Rooms)
               .Select(r => r as Room)
               .GroupBy(x => x.LevelId)
               .ToList();

            foreach (var Lvlid in rooms)
            {
                string lvlName = document.GetElement(Lvlid.Key).Name;
                lvlName = lvlName.Replace("Уровень ", " ");
                int number = 1;
                Room tmpRoom = rooms.FirstOrDefault() as Room;
                foreach (var item in Lvlid)
                {
                    item.LookupParameter("Номер").Set(lvlName + "-" + number.ToString());
                    number++;
                }
            }
        }
        public void CreateTag(Document document)
        {
            var rooms = new FilteredElementCollector(document)
              .OfCategory(BuiltInCategory.OST_Rooms)
              .Select(r => r as Room)
              .ToList();

            foreach (Room room in rooms)
            {
                //IEnumerable<RoomTag> roomTags = from elem in ((new FilteredElementCollector(document)).WherePasses(new RoomTagFilter()).ToElements())
                //                                let roomTag = elem as RoomTag
                //                                where (roomTag != null) && (roomTag.Room != null)
                //                                select roomTag;
                //foreach (RoomTag roomTagg in roomTags)
                //{
                //    if (roomTagg != null)
                //    {
                //        break;
                //    }
                //    else
                //    {
                        UV roomTagLocation = new UV(0, 0);
                        LocationPoint locationPoint = room.Location as LocationPoint;
                        UV point = new UV(locationPoint.Point.X, locationPoint.Point.Y);
                        LinkElementId roomId = new LinkElementId(room.Id);
                        RoomTag roomTag1 = document.Create.NewRoomTag(roomId, point, null);
                //    }
                //}

            }


        }
    }
}
