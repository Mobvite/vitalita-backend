# language: es
@sprint3 @profiles
Característica: Invitación y acceso de familiares al seguimiento de un adulto mayor
  Como cuidadora o enfermera
  Quiero invitar a un familiar y gestionar sus accesos
  Para que pueda consultar el seguimiento del adulto mayor sin depender de reportes manuales

  # Trazabilidad: cada escenario lleva la etiqueta de la user story que verifica.
  #   US11 Invitar a un familiar autorizado
  #   US22 Consultar el panel familiar de solo lectura
  #   US31 Crear cuenta de familiar mediante invitación
  #   US32 Gestionar los familiares autorizados

  Antecedentes:
    Dado que existe una cuidadora autenticada con su perfil de cuidadora
    Y que la cuidadora tiene registrado a un adulto mayor

  @US11
  Escenario: Invitación de un familiar
    Cuando la cuidadora invita al correo "carla.rodriguez@correo.pe" con el parentesco "Daughter"
    Entonces el sistema responde con el estado 201
    Y la invitación queda en estado "Pending"
    Y la invitación incluye un código con el formato "VITA-" seguido de 8 caracteres
    Y la invitación vence a los 7 días

  @US11
  Escenario: No se puede invitar dos veces al mismo correo
    Dado que el correo "carla.rodriguez@correo.pe" ya tiene una invitación vigente para el adulto mayor
    Cuando la cuidadora invita nuevamente a "carla.rodriguez@correo.pe"
    Entonces el sistema responde con el estado 409

  @US31
  Escenario: Consulta pública de una invitación vigente
    Dado que un familiar recibió una invitación vigente con un código válido
    Cuando consulta las invitaciones por ese código sin iniciar sesión
    Entonces el sistema responde con el estado 200
    Y recibe únicamente los datos mínimos de la invitación

  @US31
  Escenario: Creación de la cuenta de familiar mediante invitación
    Dado que un familiar recibió una invitación vigente para "carla.rodriguez@correo.pe"
    Cuando se registra con ese correo con el rol de familiar, crea su perfil y acepta la invitación
    Entonces la invitación queda en estado "Active"
    Y el familiar queda asociado al adulto mayor en modalidad de solo lectura

  @US31
  Esquema del escenario: Una invitación no válida no permite crear la cuenta
    Dado que existe una invitación <situación>
    Cuando se consultan las invitaciones por ese código sin iniciar sesión
    Entonces el sistema responde con el estado 200
    Y la lista está vacía porque la invitación no es válida

    Ejemplos:
      | situación       |
      | inexistente     |
      | revocada        |
      | vencida         |

  @US22
  Escenario: Consulta del seguimiento por un familiar autorizado
    Dado que un familiar tiene una invitación aceptada para el adulto mayor
    Cuando el familiar consulta el seguimiento del adulto mayor
    Entonces el sistema responde con el estado 200

  @US11 @US22
  Escenario: Restricción de modificación para el familiar
    Dado que un familiar tiene una invitación aceptada para el adulto mayor
    Cuando el familiar intenta modificar los datos del adulto mayor
    Entonces el sistema responde con el estado 403

  @US32
  Escenario: Consulta de los familiares autorizados
    Dado que el adulto mayor tiene un familiar con acceso activo
    Cuando la cuidadora consulta los accesos del adulto mayor
    Entonces el sistema presenta cada familiar con su parentesco y el estado de su acceso

  @US32
  Escenario: Revocación del acceso de un familiar
    Dado que un familiar tiene acceso activo al adulto mayor
    Cuando la cuidadora revoca ese acceso
    Entonces el acceso queda en estado "Revoked"
    Y el familiar deja de consultar la información del adulto mayor
