using DocumentFormat.OpenXml.Wordprocessing;
using EAUT_NCKH.Web.DTOs.Options;

namespace EAUT_NCKH.Web.DTOs {
    public class IndexViewPage<TDataList> {
        private List<TDataList> _dataList { get; set; } = new List<TDataList>();
        public DataTableOptions Pagination { get; set; } = new DataTableOptions();
        public List<TDataList> DataList {
            get => _dataList;
            set {
                _dataList = value ?? new List<TDataList>();
                CalculatePagination();
            }
        }
        public virtual void CalculatePagination() {
            Pagination.Start = Math.Max(1, Pagination.PageNumber - Pagination.Range);
            Pagination.End = Math.Min(Pagination.GetTotalPage(), Pagination.PageNumber + Pagination.Range);
            if (DataList.Count == 0) {
                Pagination.PageNumber = 0;
            }
        }
    }
}
