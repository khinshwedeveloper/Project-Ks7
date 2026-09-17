using Hotelbooking.Database.Models;

namespace HotelBookingMvc.Web.Services
{
    public class Room_Service
    {
        private readonly AppDbContext _db;
        public Room_Service(AppDbContext db)
        {
            _db = db;
        }

        public async Task<TblRoom> GetRoomAsync()
        {
            return await _db.TblRooms
                .Where(x => !x.IsDelete)
                .OrderBy(x =>.RoomNumber).ToListAsync;
        }
        public async Task<TblRoom?> GetRoomByIdAsync(int id)
        {
            return await _db.TblRooms.FirstOrDefaultAsync(x => x.RoomId == id && !x.IsDelete);

        }
        public async Task <bool> Create RoomAsync(TblRoom room)
        {
            try
            {
                int maxId = await _db.TblRooms
                    .Select(x => (int?)x.RoomId).MaxAsync() ?? 0;
                room.RoomId = maxId + 1;
                room.CreatedDateTime = DateTime.Now;
                room.IsDelete = false;
                _db.TblRooms.Add(room);
                await _db.SaveChangesAsync();
                return true;
                
            }
            catch
            {
                return false;
            }
        }
    }
}
