-- Procedimiento: GetAllCustomers
CREATE PROCEDURE [dbo].[GetAllCustomers]
AS
BEGIN
    SET NOCOUNT ON; -- Mejora rendimiento al evitar mensajes de "filas afectadas"
    
    BEGIN TRY
        SELECT 
            CUSTOMER_ID,
            EMAIL_ADDRESS,
            FULL_NAME
        FROM 
            CUSTOMERS
        ORDER BY 
            CUSTOMER_ID;
    END TRY
    BEGIN CATCH
        -- Log del error (opcionalmente puedes insertar en tabla de logs)
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        
        -- Usar THROW en lugar de RAISERROR (más moderno)
        THROW 50002, @ErrorMessage, @ErrorSeverity;
    END CATCH
END;
GO

-- Procedimiento: GetByIdCustomer
CREATE PROCEDURE [dbo].[GetByIdCustomer]
    @p_customer_id INT
AS
BEGIN
    SET NOCOUNT ON;  -- Mejora de rendimiento
    
    BEGIN TRY
        SELECT 
            CUSTOMER_ID,
            EMAIL_ADDRESS,
            FULL_NAME
        FROM 
            CUSTOMERS
        WHERE 
            CUSTOMER_ID = @p_customer_id;
            
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        
        THROW 50001, @ErrorMessage, @ErrorSeverity;
    END CATCH
END;
GO

-- Procedimiento: AddCustomer
CREATE PROCEDURE [dbo].[AddCustomer]
    @p_email_address NVARCHAR(255),
    @p_full_name NVARCHAR(255),
    @p_customer_id INT OUTPUT
AS
BEGIN
    BEGIN TRY
        -- Validación básica
        IF @p_email_address IS NULL OR @p_full_name IS NULL
            THROW 50001, 'Email y nombre son requeridos', 1;
        
        -- Insertar nuevo registro
        INSERT INTO CUSTOMERS (EMAIL_ADDRESS, FULL_NAME)
        VALUES (@p_email_address, @p_full_name);
        
        -- Obtener el ID generado
        SET @p_customer_id = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() = 2627  -- Violación de unique key (email duplicado)
            THROW 50002, 'El email ya está registrado', 1;
        ELSE
            THROW 50003, 'Error al agregar cliente', 1;
    END CATCH
END;
GO

-- Procedimiento: UpdateCustomer
CREATE PROCEDURE [dbo].[UpdateCustomer]
    @p_customer_id INT,
    @p_email_address NVARCHAR(255),
    @p_full_name NVARCHAR(255),
    @p_rows_updated INT OUTPUT
AS
BEGIN
    BEGIN TRY
        -- Validación básica
        IF @p_email_address IS NULL OR @p_full_name IS NULL
            THROW 50001, 'Email y nombre son requeridos', 1;
        
        -- Actualizar registro
        UPDATE CUSTOMERS SET
            EMAIL_ADDRESS = @p_email_address,
            FULL_NAME = @p_full_name
        WHERE CUSTOMER_ID = @p_customer_id;
        
        -- Verificar si se actualizó
        SET @p_rows_updated = @@ROWCOUNT;
        
        IF @p_rows_updated = 0
            THROW 50004, 'Cliente no encontrado', 1;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() = 2627
            THROW 50002, 'El email ya está registrado', 1;
        ELSE
            THROW 50003, 'Error al actualizar cliente', 1;
    END CATCH
END;
GO

-- Procedimiento: DeleteCustomer
CREATE PROCEDURE [dbo].[DeleteCustomer]
    @p_customer_id INT,
    @p_rows_deleted INT OUTPUT
AS
BEGIN
    BEGIN TRY
        -- Eliminar registro
        DELETE FROM CUSTOMERS
        WHERE CUSTOMER_ID = @p_customer_id;
        
        -- Verificar si se eliminó
        SET @p_rows_deleted = @@ROWCOUNT;
        
        IF @p_rows_deleted = 0
            THROW 50004, 'Cliente no encontrado', 1;
    END TRY
    BEGIN CATCH
        THROW 50005, 'Error al eliminar cliente', 1;
    END CATCH
END;
GO