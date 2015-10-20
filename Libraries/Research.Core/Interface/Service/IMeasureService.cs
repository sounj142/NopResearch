using System.Collections.Generic;
using Research.Core.Domain.Directory;

namespace Research.Core.Interface.Service
{
    /// <summary>
    /// Service chịu quản lý và đổi đơn vị đo chiều dài ( bảng MeasureDimension ), bao gồm các đơn vị như inch, mét, milimet, ..v.v..
    /// Quản lý và đổi đơn vị đo khối lượng (MeasureWeight)
    /// Ko như Nop, ta sẽ static cache toàn bộ dữ liệu MeasureDimension, MeasureWeight ( cũng chỉ có vài dòng )
    /// </summary>
    public partial interface IMeasureService
    {
        void Delete(MeasureDimension entity);


        /// <summary>
        /// per request cache
        /// </summary>
        MeasureDimension GetMeasureDimensionById(int id, bool getFromStaticCache = true);

        /// <summary>
        /// per request cache
        /// </summary>
        MeasureDimension GetMeasureDimensionBySystemKeyword(string systemKeyword, bool getFromStaticCache = true);

        /// <summary>
        /// per request cache
        /// </summary>
        IList<MeasureDimension> GetAllMeasureDimensions(bool getFromStaticCache = true);

        void Insert(MeasureDimension entity);

        void Update(MeasureDimension entity);

        /// <summary>
        /// Chuyển quantity đơn vị chiều dài loại sourceMeasureDimension thành loại targetMeasureDimension
        /// </summary>
        decimal ConvertDimension(decimal quantity, MeasureDimension sourceMeasureDimension,
            MeasureDimension targetMeasureDimension, bool round = true, int digitNum = 2);

        /// <summary>
        /// Chuyển quantity đơn vị chiều dài loại sourceMeasureDimension thành loại đơn vị chiều dài mặc định của hệ thống. Đơn
        /// vị chiều dài mặc định được qui định trong mục setting measureSettings.BaseDimensionId
        /// </summary>
        decimal ConvertToPrimaryMeasureDimension(decimal quantity, MeasureDimension sourceMeasureDimension);

        /// <summary>
        /// Chuyển quantity đơn vị chiều dài mặc định của hệ thống thành loại đơn vị chiều dài loại targetMeasureDimension. Đơn
        /// vị chiều dài mặc định được qui định trong mục setting measureSettings.BaseDimensionId
        /// </summary>
        decimal ConvertFromPrimaryMeasureDimension(decimal quantity, MeasureDimension targetMeasureDimension);

        void Delete(MeasureWeight measureWeight);

        /// <summary>
        /// per request cache
        /// </summary>
        MeasureWeight GetMeasureWeightById(int measureWeightId, bool getFromStaticCache = true);

        /// <summary>
        /// per request cache
        /// </summary>
        MeasureWeight GetMeasureWeightBySystemKeyword(string systemKeyword, bool getFromStaticCache = true);

        /// <summary>
        /// per request cache
        /// </summary>
        IList<MeasureWeight> GetAllMeasureWeights(bool getFromStaticCache = true);

        void Insert(MeasureWeight entity);

        void Update(MeasureWeight entity);

        /// <summary>
        /// Chuyển quantity đơn vị khối lượng loại sourceMeasureWeight thành loại targetMeasureWeight
        /// </summary>
        decimal ConvertWeight(decimal quantity, MeasureWeight sourceMeasureWeight,
            MeasureWeight targetMeasureWeight, bool round = true, int digitNum = 2);

        /// <summary>
        /// Chuyển quantity đơn vị khối lượng loại sourceMeasureWeight thành loại đơn vị khối lượng mặc định của hệ thống. 
        /// </summary>
        decimal ConvertToPrimaryMeasureWeight(decimal quantity, MeasureWeight sourceMeasureWeight);

        decimal ConvertFromPrimaryMeasureWeight(decimal quantity, MeasureWeight targetMeasureWeight);
    }
}