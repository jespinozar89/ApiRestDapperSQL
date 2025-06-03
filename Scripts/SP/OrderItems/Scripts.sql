-- 1. Obtener ítem por ID
CREATE PROCEDURE dbo.GetOrderItemById
    @p_order_id INT,
    @p_line_item_id INT
AS
BEGIN
    BEGIN TRY
        SELECT 
            ORDER_ID,
            LINE_ITEM_ID,
            PRODUCT_ID,
            UNIT_PRICE,
            QUANTITY,
            SHIPMENT_ID
        FROM ORDERS_ITEMS
        WHERE ORDER_ID = @p_order_id
          AND LINE_ITEM_ID = @p_line_item_id;
    END TRY
    BEGIN CATCH
        THROW 51010, 'Error en GetOrderItemById', 1;
    END CATCH
END;
GO

-- 2. Obtener todos los ítems
CREATE PROCEDURE dbo.GetAllOrderItems
AS
BEGIN
    BEGIN TRY
        SELECT 
            ORDER_ID,
            LINE_ITEM_ID,
            PRODUCT_ID,
            UNIT_PRICE,
            QUANTITY,
            SHIPMENT_ID
        FROM ORDERS_ITEMS
        ORDER BY ORDER_ID, LINE_ITEM_ID;
    END TRY
    BEGIN CATCH
        THROW 51020, 'Error en GetAllOrderItems', 1;
    END CATCH
END;
GO

-- 3. Agregar nuevo ítem
CREATE PROCEDURE dbo.AddOrderItem
    @p_order_id INT,
    @p_line_item_id INT,
    @p_product_id INT,
    @p_unit_price DECIMAL(18,2),
    @p_quantity INT,
    @p_shipment_id INT = NULL,
    @p_rows_inserted INT OUTPUT
AS
BEGIN
    BEGIN TRY
        -- Validaciones de parámetros
        IF @p_order_id IS NULL
            THROW 51030, 'ORDER_ID es requerido', 1;
        IF @p_line_item_id IS NULL
            THROW 51031, 'LINE_ITEM_ID es requerido', 1;
        IF @p_product_id IS NULL
            THROW 51032, 'PRODUCT_ID es requerido', 1;
        IF @p_unit_price IS NULL
            THROW 51033, 'UNIT_PRICE es requerido', 1;
        IF @p_quantity IS NULL
            THROW 51034, 'QUANTITY es requerido', 1;

        -- Insertar registro
        INSERT INTO ORDERS_ITEMS (
            ORDER_ID,
            LINE_ITEM_ID,
            PRODUCT_ID,
            UNIT_PRICE,
            QUANTITY,
            SHIPMENT_ID
        ) VALUES (
            @p_order_id,
            @p_line_item_id,
            @p_product_id,
            @p_unit_price,
            @p_quantity,
            @p_shipment_id
        );

        SET @p_rows_inserted = @@ROWCOUNT;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() = 2627  -- Violación de clave única (duplicado)
            THROW 51035, 'Elemento duplicado en ORDERS_ITEMS', 1;
        ELSE
            THROW 51036, 'Error al añadir OrderItem', 1;
    END CATCH
END;
GO

-- 4. Actualizar ítem
CREATE PROCEDURE dbo.UpdateOrderItem
    @p_order_id INT,
    @p_line_item_id INT,
    @p_product_id INT,
    @p_unit_price DECIMAL(18,2),
    @p_quantity INT,
    @p_shipment_id INT = NULL,
    @p_rows_updated INT OUTPUT
AS
BEGIN
    BEGIN TRY
        -- Validaciones básicas
        IF @p_order_id IS NULL
            THROW 51040, 'ORDER_ID es requerido', 1;
        IF @p_line_item_id IS NULL
            THROW 51041, 'LINE_ITEM_ID es requerido', 1;

        -- Verificar existencia
        IF NOT EXISTS (
            SELECT 1 
            FROM ORDERS_ITEMS 
            WHERE ORDER_ID = @p_order_id 
              AND LINE_ITEM_ID = @p_line_item_id
        )
            THROW 51042, 'Registro no encontrado', 1;

        -- Actualizar registro
        UPDATE ORDERS_ITEMS SET
            PRODUCT_ID  = @p_product_id,
            UNIT_PRICE  = @p_unit_price,
            QUANTITY    = @p_quantity,
            SHIPMENT_ID = @p_shipment_id
        WHERE ORDER_ID = @p_order_id
          AND LINE_ITEM_ID = @p_line_item_id;

        SET @p_rows_updated = @@ROWCOUNT;
    END TRY
    BEGIN CATCH
        THROW 51043, 'Error al actualizar OrderItem', 1;
    END CATCH
END;
GO

-- 5. Eliminar ítem
CREATE PROCEDURE dbo.DeleteOrderItem
    @p_order_id INT,
    @p_line_item_id INT,
    @p_rows_deleted INT OUTPUT
AS
BEGIN
    BEGIN TRY
        -- Validaciones básicas
        IF @p_order_id IS NULL
            THROW 51050, 'ORDER_ID es requerido', 1;
        IF @p_line_item_id IS NULL
            THROW 51051, 'LINE_ITEM_ID es requerido', 1;

        -- Verificar existencia
        IF NOT EXISTS (
            SELECT 1 
            FROM ORDERS_ITEMS 
            WHERE ORDER_ID = @p_order_id 
              AND LINE_ITEM_ID = @p_line_item_id
        )
            THROW 51052, 'Registro no encontrado', 1;

        -- Eliminar registro
        DELETE FROM ORDERS_ITEMS
        WHERE ORDER_ID = @p_order_id
          AND LINE_ITEM_ID = @p_line_item_id;

        SET @p_rows_deleted = @@ROWCOUNT;
    END TRY
    BEGIN CATCH
        THROW 51053, 'Error al eliminar OrderItem', 1;
    END CATCH
END;
GO