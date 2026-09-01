using Microsoft.Data.SqlClient;
using Parquet;
using Parquet.Schema;
using Parquet.Serialization;
using Parquet2CSV;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;

string vConnectionString = "Data Source=BI-SQL-DC;Initial Catalog=DWH_STAGE_COMMON;Integrated Security=SSPI;Encrypt=False;";

SqlConnection oSqlConnection = new SqlConnection(vConnectionString);
oSqlConnection.Open();
SqlCommand oSqlCommand = new SqlCommand("SELECT TableName, [id], [Parquet]  FROM [Mindbox].[LogParquetToCSV] (NOLOCK) WHERE [sign_parquet] = 1 AND [sign_csv] = 0 ORDER BY TableName, Version", oSqlConnection);
oSqlCommand.CommandTimeout = 120;


SqlConnection oSqlConnectionTarget = new SqlConnection(vConnectionString);
oSqlConnectionTarget.Open();
SqlCommand oSqlCommandUpdate = new SqlCommand("UPDATE [Mindbox].[LogParquetToCSV] SET CSV = @CSV, SIGN_CSV = 1, SYSMOMENT = GETDATE() WHERE TableName = @TableName AND [id] = @id",oSqlConnectionTarget);
oSqlCommandUpdate.CommandTimeout = 120;
oSqlCommandUpdate.Parameters.Add("@CSV", SqlDbType.Image);
oSqlCommandUpdate.Parameters.Add("@TableName", SqlDbType.NVarChar, 200);
oSqlCommandUpdate.Parameters.Add("@id", SqlDbType.NVarChar, 100);

SqlDataReader oSqlDataReader = oSqlCommand.ExecuteReader();

while (oSqlDataReader.Read())
{
    int bufferSize = 1000000;                   // Size of the BLOB buffer.
    byte[] outbyte = new byte[bufferSize];  // The BLOB byte[] buffer to be filled by GetBytes.
    long retval;                            // The bytes returned from GetBytes.
    // Get the total length of the BLOB data
    long totalBytes = oSqlDataReader.GetBytes(2, 0, null, 0, 0);

    // Create a buffer to read chunks of data
    long startIndex = 0;
    int bytesRead;

    MemoryStream memoryStream = new MemoryStream();

    // Read the BLOB data in chunks and write to the MemoryStream
    while ((bytesRead = (int)oSqlDataReader.GetBytes(2, startIndex, outbyte, 0, bufferSize)) > 0)
    {
        memoryStream.Write(outbyte, 0, bytesRead);
        startIndex += bytesRead;
    }

    // Ensure the stream position is at the beginning for deserialization
    memoryStream.Position = 0;
    byte[] oBuffer = null;

    switch (oSqlDataReader["TableName"])
    {
        case "exports.CDP.Segmentations":
            IEnumerable<exports_CDP_Segmentations> records1 = (IEnumerable<exports_CDP_Segmentations>)await ParquetSerializer.DeserializeAsync<exports_CDP_Segmentations>(memoryStream);
            oBuffer = CSV.ToCsv<exports_CDP_Segmentations>(";", records1);
            break;

        case "exports.CDP.Segments":
            IEnumerable<exports_CDP_Segments> records2 = (IEnumerable<exports_CDP_Segments>)await ParquetSerializer.DeserializeAsync<exports_CDP_Segments>(memoryStream);
            oBuffer = CSV.ToCsv<exports_CDP_Segments>(";", records2);
            break;

        case "exports.CDP.CustomerSegmentHistory":
            IEnumerable<exports_CDP_CustomerSegmentHistory> records3 = (IEnumerable<exports_CDP_CustomerSegmentHistory>)await ParquetSerializer.DeserializeAsync<exports_CDP_CustomerSegmentHistory>(memoryStream);
            oBuffer = CSV.ToCsv<exports_CDP_CustomerSegmentHistory>(";", records3);
            break;

        case "exports.ProcessingOrders.BonusPointChanges":
            IEnumerable<exports_ProcessingOrders_BonusPointChanges> records4 = (IEnumerable<exports_ProcessingOrders_BonusPointChanges>)await ParquetSerializer.DeserializeAsync<exports_ProcessingOrders_BonusPointChanges>(memoryStream);
            oBuffer = CSV.ToCsv<exports_ProcessingOrders_BonusPointChanges>(";", records4);
            break;

        case "exports.ProcessingOrders.BonusPointsMechanics":
            IEnumerable<exports_ProcessingOrders_BonusPointsMechanics> records5 = (IEnumerable<exports_ProcessingOrders_BonusPointsMechanics>)await ParquetSerializer.DeserializeAsync<exports_ProcessingOrders_BonusPointsMechanics>(memoryStream);
            oBuffer = CSV.ToCsv<exports_ProcessingOrders_BonusPointsMechanics>(";", records5);
            break;
    }


    oSqlCommandUpdate.Parameters["@CSV"].Value = Compressor.Compress( oBuffer );
    oSqlCommandUpdate.Parameters["@TableName"].Value = oSqlDataReader["TableName"];
    oSqlCommandUpdate.Parameters["@id"].Value = oSqlDataReader["id"];
    oSqlCommandUpdate.ExecuteNonQuery();
}

oSqlDataReader.Close();

oSqlConnectionTarget.Close();
oSqlConnection.Close();






