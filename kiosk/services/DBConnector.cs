using System;
using Microsoft.Data.SqlClient;
using System.IO;
using s_oil.Utils;
using System.Collections.Generic;
using s_oil.models;
using System.Windows.Forms;
using System.Linq;

namespace s_oil.Services
{
    public class DBConnector
    {
        private static readonly Lazy<DBConnector> instance = new Lazy<DBConnector>(() => new DBConnector());
        private readonly string connectionString;

        private DBConnector()
        {
            var iniPath = Path.Combine(Application.StartupPath, "SMT_Kiosk.ini");
            var iniParser = new IniParser(iniPath);
            var dbHost = iniParser.GetSetting("Database", "DB_IP");
            var dbUser = iniParser.GetSetting("Database", "DB_USER");
            var dbPassword = iniParser.GetSetting("Database", "DB_PASSWORD");
            var dbCatalog = iniParser.GetSetting("Database", "DB_CTALOG");

            connectionString = $"Data Source={dbHost};Initial Catalog={dbCatalog};User ID={dbUser};Password={dbPassword};TrustServerCertificate=True;";
        }

        public static DBConnector Instance => instance.Value;

        public SqlConnection GetConnection()
        {
            var connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                Logger.Info("Database connection successful!");
            }
            catch (Exception ex)
            {
                Logger.Error("Database connection failed", ex);
            }
            return connection;
        }

        public List<BoxMaster> GetBoxesByAreaCode(string areaCode)
        {
            var boxes = new List<BoxMaster>();
            string query = "SELECT * FROM tblBoxMaster WHERE areaCode = @areaCode ORDER BY boxNo asc";

            using (var connection = GetConnection())
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    Logger.Warning("Could not get boxes, database connection is not open.");
                    return boxes;
                }

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@areaCode", areaCode);

                    try
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var box = new BoxMaster
                                {
                                    areaCode = reader["areaCode"].ToString(),
                                    BoxNo = Convert.ToInt32(reader["boxNo"]),
                                    ServiceType = Convert.ToInt32(reader["serviceType"]),
                                    BoxSizeType = Convert.ToInt32(reader["boxSizeType"]),
                                    useState = Convert.ToInt32(reader["useState"]),
                                    userCode = reader["userCode"].ToString(),
                                    userName = reader["userName"].ToString(),
                                    userPhone = reader["userPhone"].ToString(),
                                    payType = reader["payType"] != DBNull.Value ? Convert.ToInt32(reader["payType"]) : 0,
                                    dong = reader["dong"].ToString(),
                                    addressNum = reader["addressNum"].ToString(),
                                    transCode = reader["transCode"].ToString(),
                                    transPhone = reader["transPhone"].ToString(),
                                    barcode = reader["barcode"].ToString(),
                                    boxPassword = reader["boxPassword"].ToString(),
                                    payCode = reader["payCode"].ToString(),
                                    payAmount = reader["payAmount"].ToString(),
                                    userTimeType = reader["useTimeType"] != DBNull.Value ? Convert.ToInt32(reader["useTimeType"]) : 0,
                                    startTime = reader["startTime"].ToString(),
                                    endTime = reader["endTime"].ToString(),
                                    productCode = reader["productCode"].ToString()
                                };
                                boxes.Add(box);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to execute query to get boxes for areaCode {areaCode}.", ex);
                    }
                }
            }
            return boxes;
        }

        public List<Product> GetProducts(string areaCode)
        {
            var products = new List<Product>();
            string query = "SELECT productCode, productName, productPrice, productType FROM tblProduct";

            using (var connection = GetConnection())
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    Logger.Warning("Could not get products, database connection is not open.");
                    return products;
                }

                using (var command = new SqlCommand(query, connection))
                {
                    try
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var product = new Product
                                {
                                    productCode = reader["productCode"].ToString(),
                                    productName = reader["productName"].ToString(),
                                    price = Convert.ToInt32(reader["productPrice"]),
                                    productType = Convert.ToInt32(reader["productType"])
                                };
                                products.Add(product);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to execute query to get products for areaCode {areaCode}.", ex);
                    }
                }
            }
            return products;
        }

        public List<BoxMaster> GetBoxesWithProductsByAreaCode(string areaCode)
        {
            var boxes = new List<BoxMaster>();
            string query = @"
                SELECT 
                    bm.areaCode,
                    bm.boxNo,
                    bm.serviceType,
                    bm.boxSizeType,
                    bm.useState,
                    bm.userCode,
                    bm.userName,
                    bm.userPhone,
                    bm.payType,
                    bm.dong,
                    bm.addressNum,
                    bm.transCode,
                    bm.transPhone,
                    bm.barcode,
                    bm.boxPassword,
                    bm.payCode,
                    bm.payAmount,
                    bm.useTimeType,
                    bm.startTime,
                    bm.endTime,
                    bm.productCode,
                    ISNULL(p.productName, '') as productName,
                    ISNULL(p.productPrice, 0) as productPrice,
                    ISNULL(p.productType, 0) as productType
                FROM tblBoxMaster bm
                LEFT JOIN tblProduct p ON bm.productCode = p.productCode
                WHERE bm.areaCode = @areaCode
                ORDER BY bm.boxNo ASC";

            using (var connection = GetConnection())
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    Logger.Warning("Could not get boxes with products, database connection is not open.");
                    return boxes;
                }

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@areaCode", areaCode);

                    try
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var box = new BoxMaster
                                {
                                    areaCode = reader["areaCode"].ToString(),
                                    BoxNo = Convert.ToInt32(reader["boxNo"]),
                                    ServiceType = Convert.ToInt32(reader["serviceType"]),
                                    BoxSizeType = Convert.ToInt32(reader["boxSizeType"]),
                                    useState = Convert.ToInt32(reader["useState"]),
                                    userCode = reader["userCode"].ToString(),
                                    userName = reader["userName"].ToString(),
                                    userPhone = reader["userPhone"].ToString(),
                                    payType = reader["payType"] != DBNull.Value ? Convert.ToInt32(reader["payType"]) : 0,
                                    dong = reader["dong"].ToString(),
                                    addressNum = reader["addressNum"].ToString(),
                                    transCode = reader["transCode"].ToString(),
                                    transPhone = reader["transPhone"].ToString(),
                                    barcode = reader["barcode"].ToString(),
                                    boxPassword = reader["boxPassword"].ToString(),
                                    payCode = reader["payCode"].ToString(),
                                    payAmount = reader["payAmount"].ToString(),
                                    userTimeType = reader["useTimeType"] != DBNull.Value ? Convert.ToInt32(reader["useTimeType"]) : 0,
                                    startTime = reader["startTime"].ToString(),
                                    endTime = reader["endTime"].ToString(),
                                    productCode = reader["productCode"].ToString(),
                                    // JOIN으로 가져온 제품 정보
                                    productName = reader["productName"].ToString(),
                                    price = Convert.ToInt32(reader["productPrice"]),
                                    productType = Convert.ToInt32(reader["productType"])
                                };
                                boxes.Add(box);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to execute JOIN query to get boxes with products for areaCode {areaCode}.", ex);
                    }
                }
            }
            
            return boxes;
        }

        /// <summary>
        /// 사물함에 상품을 배치하고 상태를 업데이트합니다 (관리자용)
        /// </summary>
        public bool UpdateBoxProductAssignment(string areaCode, int boxNo, string productCode, int useState = 2)
        {
            string query = @"
                UPDATE tblBoxMaster 
                SET productCode = @productCode, useState = @useState,
                    userCode = '', userName = '', userPhone = '',
                    startTime = '', endTime = '', payCode = '', payAmount = ''
                WHERE areaCode = @areaCode AND boxNo = @boxNo";

            using (var connection = GetConnection())
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    Logger.Warning("Could not update box, database connection is not open.");
                    return false;
                }

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@areaCode", areaCode);
                    command.Parameters.AddWithValue("@boxNo", boxNo);
                    command.Parameters.AddWithValue("@productCode", productCode);
                    command.Parameters.AddWithValue("@useState", useState);

                    try
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                        bool success = rowsAffected > 0;
                        
                        if (success)
                        {
                            Logger.Info($"Box {boxNo} updated successfully with product {productCode}");
                        }
                        else
                        {
                            Logger.Warning($"No rows affected when updating box {boxNo}");
                        }
                        
                        return success;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to update box {boxNo} with product {productCode}", ex);
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// 사물함 상태를 변경합니다 (관리자용)
        /// </summary>
        public bool UpdateBoxState(string areaCode, int boxNo, int useState)
        {
            string query = @"
                UPDATE tblBoxMaster 
                SET useState = @useState
                WHERE areaCode = @areaCode AND boxNo = @boxNo";

            using (var connection = GetConnection())
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    Logger.Warning("Could not update box state, database connection is not open.");
                    return false;
                }

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@areaCode", areaCode);
                    command.Parameters.AddWithValue("@boxNo", boxNo);
                    command.Parameters.AddWithValue("@useState", useState);

                    try
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                        bool success = rowsAffected > 0;
                        
                        if (success)
                        {
                            string stateText = useState == 1 ? "사용불가" : "사용가능";
                            Logger.Info($"Box {boxNo} state updated to {stateText}");
                        }
                        else
                        {
                            Logger.Warning($"No rows affected when updating box {boxNo} state");
                        }
                        
                        return success;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to update box {boxNo} state", ex);
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// 사물함에서 상품을 제거합니다 (관리자용)
        /// </summary>
        public bool RemoveProductFromBox(string areaCode, int boxNo)
        {
            string query = @"
                UPDATE tblBoxMaster 
                SET productCode = '', useState = 2,
                    userCode = '', userName = '', userPhone = '',
                    startTime = '', endTime = '', payCode = '', payAmount = ''
                WHERE areaCode = @areaCode AND boxNo = @boxNo";

            using (var connection = GetConnection())
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    Logger.Warning("Could not remove product from box, database connection is not open.");
                    return false;
                }

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@areaCode", areaCode);
                    command.Parameters.AddWithValue("@boxNo", boxNo);

                    try
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                        bool success = rowsAffected > 0;
                        
                        if (success)
                        {
                            Logger.Info($"Product removed from box {boxNo} successfully");
                        }
                        else
                        {
                            Logger.Warning($"No rows affected when removing product from box {boxNo}");
                        }
                        
                        return success;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to remove product from box {boxNo}", ex);
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// 결제 정보를 데이터베이스에 저장합니다
        /// </summary>
        public bool SavePayment(models.Payment payment)
        {
            // 저장 전 데이터 검증 및 정리
            var cleanPayment = new models.Payment
            {
                areaCode = TruncateString(payment.areaCode, 20),
                boxNo = payment.boxNo,
                userCode = TruncateString(payment.userCode ?? $"U{DateTime.Now:MMddHHmmss}", 20),
                payType = payment.payType,
                payAmount = payment.payAmount,
                payPhone = TruncateString(payment.payPhone ?? "", 20),
                confirmKey = TruncateString(payment.confirmKey ?? "", 20),
                cardNumber = TruncateString(payment.cardNumber ?? "****", 20),
                payTime = payment.payTime ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // 기본 필드를 사용하여 저장 시도
            string query = @"
                INSERT INTO tblPayment (
                    areaCode, boxNo, userCode, payType, payAmount, payPhone, 
                    confirmKey, cardNumber, payTime
                ) VALUES (
                    @areaCode, @boxNo, @userCode, @payType, @payAmount, @payPhone,
                    @confirmKey, @cardNumber, @payTime
                )";

            using (var connection = GetConnection())
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    Logger.Warning("Could not save payment, database connection is not open.");
                    return false;
                }

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@areaCode", cleanPayment.areaCode);
                    command.Parameters.AddWithValue("@boxNo", cleanPayment.boxNo);
                    command.Parameters.AddWithValue("@userCode", cleanPayment.userCode);
                    command.Parameters.AddWithValue("@payType", cleanPayment.payType);
                    command.Parameters.AddWithValue("@payAmount", cleanPayment.payAmount);
                    command.Parameters.AddWithValue("@payPhone", cleanPayment.payPhone);
                    command.Parameters.AddWithValue("@confirmKey", cleanPayment.confirmKey);
                    command.Parameters.AddWithValue("@cardNumber", cleanPayment.cardNumber);
                    command.Parameters.AddWithValue("@payTime", cleanPayment.payTime);

                    try
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                        bool success = rowsAffected > 0;
                        
                        if (success)
                        {
                            Logger.Info($"Payment saved successfully - BoxNo: {payment.boxNo}, Amount: {payment.payAmount:N0}원");
                        }
                        else
                        {
                            Logger.Warning($"No rows affected when saving payment for box {payment.boxNo}");
                        }
                        
                        return success;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to save payment for box {payment.boxNo}: {ex.Message}", ex);
                        
                        // 오류가 발생하면 더 기본적인 형태로 재시도
                        return SavePaymentFallback(cleanPayment);
                    }
                }
            }
        }

        /// <summary>
        /// 결제 정보 저장 실패 시 대체 방법 (최소한의 필드만 사용) - 개선된 버전
        /// </summary>
        private bool SavePaymentFallback(models.Payment payment)
        {
            try
            {
                //  가장 기본적인 필드만 사용하되 NULL 방지 처리
                string query = @"
                    INSERT INTO tblPayment (areaCode, boxNo, payType, payAmount, payTime, userCode) 
                    VALUES (@areaCode, @boxNo, @payType, @payAmount, @payTime, @userCode)";

                using (var connection = GetConnection())
                {
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        Logger.Warning("Could not save payment (fallback), database connection is not open.");
                        return false;
                    }

                    using (var command = new SqlCommand(query, connection))
                    {
                        // ?? 모든 문자열 필드에 대해 NULL 방지 및 길이 제한 처리
                        command.Parameters.AddWithValue("@areaCode", TruncateString(payment.areaCode, 20));
                        command.Parameters.AddWithValue("@boxNo", payment.boxNo);
                        command.Parameters.AddWithValue("@payType", payment.payType);
                        command.Parameters.AddWithValue("@payAmount", payment.payAmount);
                        command.Parameters.AddWithValue("@payTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@userCode", TruncateString(payment.userCode ?? $"U{DateTime.Now:MMddHHmmss}", 20));

                        int rowsAffected = command.ExecuteNonQuery();
                        bool success = rowsAffected > 0;
                        
                        if (success)
                        {
                            Logger.Info($"Payment saved successfully (fallback) - BoxNo: {payment.boxNo}, Amount: {payment.payAmount:N0}원");
                        }
                        else
                        {
                            Logger.Warning($"No rows affected when saving payment (fallback) for box {payment.boxNo}");
                        }
                        
                        return success;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to save payment (fallback) for box {payment.boxNo}: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        ///  문자열 길이 제한 및 NULL 방지 유틸리티
        /// </summary>
        private string TruncateString(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            
            return input.Length <= maxLength ? input : input.Substring(0, maxLength);
        }

        /// <summary>
        /// 테이블 구조를 확인하여 존재하는 컬럼만 사용하는 쿼리 생성
        /// </summary>
        public List<string> GetTableColumns(string tableName)
        {
            var columns = new List<string>();
            string query = @"
                SELECT COLUMN_NAME 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = @tableName 
                ORDER BY ORDINAL_POSITION";

            try
            {
                using (var connection = GetConnection())
                {
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        Logger.Warning("Could not get table columns, database connection is not open.");
                        return columns;
                    }

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@tableName", tableName);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                columns.Add(reader["COLUMN_NAME"].ToString());
                            }
                        }
                    }
                }

                Logger.Info($"Table {tableName} columns: {string.Join(", ", columns)}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to get columns for table {tableName}", ex);
            }

            return columns;
        }

        /// <summary>
        /// 동적으로 테이블 구조에 맞는 결제 정보 저장
        /// </summary>
        public bool SavePaymentDynamic(models.Payment payment)
        {
            try
            {
                // 테이블 구조 확인
                var columns = GetTableColumns("tblPayment");
                if (columns.Count == 0)
                {
                    Logger.Warning("Could not retrieve table structure for tblPayment");
                    return false;
                }

                // 사용 가능한 컬럼과 값 매핑
                var fieldValues = new Dictionary<string, object>
                {
                    { "areaCode", payment.areaCode },
                    { "boxNo", payment.boxNo },
                    { "userCode", payment.userCode ?? "" },
                    { "payType", payment.payType },
                    { "payAmount", payment.payAmount },
                    { "payPhone", payment.payPhone ?? "" },
                    { "confirmKey", payment.confirmKey ?? "" },
                    { "cardNumber", payment.cardNumber ?? "" },
                    { "payTime", payment.payTime ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                    { "approvalNumber", payment.approvalNumber ?? "" },
                    { "payStatus", payment.payStatus },
                    { "errorMessage", payment.errorMessage ?? "" }
                };

                // 실제 존재하는 컬럼만 필터링
                var availableFields = fieldValues.Where(kv => columns.Contains(kv.Key, StringComparer.OrdinalIgnoreCase)).ToList();

                if (availableFields.Count == 0)
                {
                    Logger.Warning("No matching columns found for payment insertion");
                    return false;
                }

                // 동적 쿼리 생성
                var columnNames = string.Join(", ", availableFields.Select(f => f.Key));
                var parameterNames = string.Join(", ", availableFields.Select(f => "@" + f.Key));
                string query = $"INSERT INTO tblPayment ({columnNames}) VALUES ({parameterNames})";

                using (var connection = GetConnection())
                {
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        Logger.Warning("Could not save payment (dynamic), database connection is not open.");
                        return false;
                    }

                    using (var command = new SqlCommand(query, connection))
                    {
                        // 파라미터 추가
                        foreach (var field in availableFields)
                        {
                            command.Parameters.AddWithValue("@" + field.Key, field.Value ?? DBNull.Value);
                        }

                        int rowsAffected = command.ExecuteNonQuery();
                        bool success = rowsAffected > 0;
                        
                        if (success)
                        {
                            Logger.Info($"Payment saved successfully (dynamic) - BoxNo: {payment.boxNo}, Amount: {payment.payAmount:N0}원, Fields: {columnNames}");
                        }
                        else
                        {
                            Logger.Warning($"No rows affected when saving payment (dynamic) for box {payment.boxNo}");
                        }
                        
                        return success;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to save payment (dynamic) for box {payment.boxNo}", ex);
                return false;
            }
        }

        /// <summary>
        /// 사물함을 구매 완료 상태로 업데이트합니다
        /// </summary>
        public bool UpdateBoxAfterPayment(string areaCode, int boxNo, string userCode, string userPhone, string payCode, int payAmount)
        {
            string query = @"
                UPDATE tblBoxMaster 
                SET useState = 1
                WHERE areaCode = @areaCode AND boxNo = @boxNo";

            using (var connection = GetConnection())
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    Logger.Warning("Could not update box after payment, database connection is not open.");
                    return false;
                }

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@areaCode", areaCode);
                    command.Parameters.AddWithValue("@boxNo", boxNo);
                    command.Parameters.AddWithValue("@startTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    try
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                        bool success = rowsAffected > 0;
                        
                        if (success)
                        {
                            Logger.Info($"Box {boxNo} updated after payment - User: {userCode}, Amount: {payAmount:N0}원");
                        }
                        else
                        {
                            Logger.Warning($"No rows affected when updating box {boxNo} after payment");
                        }
                        
                        return success;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to update box {boxNo} after payment", ex);
                        return false;
                    }
                }
            }
        }
    }
}