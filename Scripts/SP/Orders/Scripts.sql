-- 1. Obtener orden por ID
CREATE PROCEDURE dbo.GetOrderById
    @p_order_id INT
AS
BEGIN
    BEGIN TRY
        SELECT 
            ORDER_ID,
            ORDER_TMS,
            CUSTOMER_ID,
            ORDER_STATUS,
            STORE_ID
        FROM ORDERS
        WHERE ORDER_ID = @p_order_id;
    END TRY
    BEGIN CATCH
        THROW 50010, 'Error en GetOrderById', 1;
    END CATCH
END;
GO

-- 2. Obtener todas las órdenes
CREATE PROCEDURE dbo.GetAllOrders
AS
BEGIN
    BEGIN TRY
        SELECT 
            ORDER_ID,
            ORDER_TMS,
            CUSTOMER_ID,
            ORDER_STATUS,
            STORE_ID
        FROM ORDERS
        ORDER BY ORDER_ID DESC;
    END TRY
    BEGIN CATCH
        THROW 50020, 'Error en GetAllOrders', 1;
    END CATCH
END;
GO

-- 3. Agregar nueva orden
CREATE PROCEDURE dbo.AddOrder
    @p_order_tms    DATETIME = NULL,
    @p_customer_id  INT,
    @p_order_status VARCHAR(50),
    @p_store_id     INT,
    @p_order_id     INT OUTPUT
AS
BEGIN
    BEGIN TRY
        -- Validación de parámetros
        IF @p_customer_id IS NULL
            THROW 50030, 'ID de cliente es requerido', 1;
        IF @p_order_status IS NULL
            THROW 50031, 'Estado de la orden es requerido', 1;
        IF @p_store_id IS NULL
            THROW 50032, 'ID de tienda es requerido', 1;

        -- Insertar con valor por defecto para fecha
        INSERT INTO ORDERS (
            ORDER_TMS,
            CUSTOMER_ID,
            ORDER_STATUS,
            STORE_ID
        ) VALUES (
            ISNULL(@p_order_tms, GETDATE()),
            @p_customer_id,
            @p_order_status,
            @p_store_id
        );
        
        SET @p_order_id = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() = 2627  -- Violación de unique key
            THROW 50033, 'Orden duplicada', 1;
        ELSE
            THROW 50034, 'Error al crear orden', 1;
    END CATCH
END;
GO

-- 4. Actualizar orden existente
CREATE PROCEDURE dbo.UpdateOrder
    @p_order_id      INT,
    @p_order_tms     DATETIME = NULL,
    @p_customer_id   INT,
    @p_order_status  VARCHAR(50),
    @p_store_id      INT,
    @p_rows_updated  INT OUTPUT
AS
BEGIN
    BEGIN TRY
        -- Validación de parámetros
        IF @p_customer_id IS NULL
            THROW 50040, 'ID de cliente es requerido', 1;

        -- Actualización condicional
        UPDATE ORDERS SET
            ORDER_TMS = ISNULL(@p_order_tms, ORDER_TMS),
            CUSTOMER_ID = @p_customer_id,
            ORDER_STATUS = @p_order_status,
            STORE_ID = @p_store_id
        WHERE ORDER_ID = @p_order_id;

        SET @p_rows_updated = @@ROWCOUNT;
        
        IF @p_rows_updated = 0
            THROW 50043, 'Orden no encontrada', 1;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() = 2627
            THROW 50033, 'Orden duplicada', 1;
        ELSE
            THROW 50044, 'Error al actualizar orden', 1;
    END CATCH
END;
GO

-- 5. Eliminar orden
CREATE PROCEDURE dbo.DeleteOrder
    @p_order_id     INT,
    @p_rows_deleted INT OUTPUT
AS
BEGIN
    BEGIN TRY
        DELETE FROM ORDERS
        WHERE ORDER_ID = @p_order_id;
        
        SET @p_rows_deleted = @@ROWCOUNT;
        
        IF @p_rows_deleted = 0
            THROW 50051, 'Orden no encontrada', 1;
    END TRY
    BEGIN CATCH
        THROW 50052, 'Error al eliminar orden', 1;
    END CATCH
END;
GO