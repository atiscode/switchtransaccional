function actualiza() {

    var url = $.HostUrl() + "Home/Actualiza";
    $.ajax({ url: url, data: null, type: 'GET', dataType: 'html', cache: false,
        success: function (response) {
           
        },
        error: function (xhr, status) {

        },
        complete: function (xhr, status) {
        }
    });
}


jQuery.extend({
    inicia: function () {
        if ($.appGlobales != undefined) $.appGlobales = null;      

        $(window).on("resize", function (ventana) {
          
            var sombras = $("[id^=modal_]");
            sombras.each(function (i, item) {
               $(item).css("width", ventana.currentTarget.window.innerWidth.toString() + "px");
               $(item).css("height", ventana.currentTarget.screen.height.toString() + "px");
            });
        
        });       

        $.appGlobales = {
            touch : false,
            lastClick: 0,
            contadorGuid: 1,
            fastClickCounter: 0,
            eventosArray: new Array(),
            eventos: new Array(),
            autorizeClick: function (event) {
                if (event.timeStamp - $.appGlobales.lastClick > 1000 ) {
                    $.appGlobales.lastClick = event.timeStamp;
                    return true;
                } else {
                    $.appGlobales.fastClickCounter++;
                    return false;
                }
            },
            dialogoscreados: 1,
            stack: new Array(),
            stackRefrescamiento: new Array(),
            actualMoneda: 0,
            actualplace: { idPais: 0, idProvincia: 0, idCiudad: 0, idSector: 0, idLugar: 0, Pais: "", Provincia: "", Ciudad: "", Sector: "", Lugar: "" },
            getPlace: function(required, fn, htmlTodas) {
               
                required = (required == "undefined" || required == null) ? "10100" : required;

                $.ajax({
                    url: $.HostUrl() + "Aplicacion/Ubication",
                    type: 'GET',
                    data: { PaisId: $.appGlobales.actualplace.idPais },
                    dataType: 'html',
                    cache: false,
                    success: function (response) {
                        
                        $.appGlobales.creaDialogVirtual({
                            html: response,
                            modal: true,
                            open: function (ui) {
                                $.appGlobales.actualplace.idPais = $("#ubication_PaisId").val() == "undefined" ? 0 : parseInt($("#ubication_PaisId").val());
                                $.appGlobales.actualplace.idProvincia = $("#ubication_ProvinciaId").val() == "undefined" ? 0 : parseInt($("#ubication_ProvinciaId").val());
                                $.appGlobales.actualplace.idCiudad = $("#ubication_CiudadId").val() == "undefined" ? 0 : parseInt($("#ubication_CiudadId").val());
                                $.appGlobales.actualplace.idSector = $("#ubication_SectorId").val() == "undefined" ? 0 : parseInt($("#ubication_SectorId").val());
                                $.appGlobales.actualplace.idLugar = $("#ubication_LugarId").val() == "undefined" ? 0 : parseInt($("#ubication_LugarId").val());

                                $.appGlobales.actualplace.Pais = $("#ubication_PaisId").val() == "undefined" ? "" : $("#ubication_PaisId option[value=" + $("#ubication_PaisId").val() + "]").text();
                                $.appGlobales.actualplace.Provincia = $("#ubication_ProvinciaId").val() == "undefined" ? "" : $("#ubication_ProvinciaId option[value=" + $("#ubication_ProvinciaId").val() + "]").text();
                                $.appGlobales.actualplace.Ciudad = $("#ubication_CiudadId").val() == "undefined" ? "" : $("#ubication_CiudadId option[value=" + $("#ubication_CiudadId").val() + "]").text();
                                $.appGlobales.actualplace.Sector = $("#ubication_SectorId").val() == "undefined" ? "" : $("#ubication_SectorId option[value=" + $("#ubication_SectorId").val() + "]").text();
                                $.appGlobales.actualplace.Lugar = $("#ubication_LugarId").val() == "undefined" ? "" : $("#ubication_LugarId option[value=" + $("#ubication_LugarId").val() + "]").text();
                              
                                $("#ubication_Aceptar").unbind("click");
                                $("#ubication_Aceptar").bind("click", function () {
                                 
                                    $.appGlobales.actualplace.idPais = $("#ubication_PaisId").val() == "undefined" ? 0 : parseInt($("#ubication_PaisId").val());
                                    $.appGlobales.actualplace.idProvincia = $("#ubication_ProvinciaId").val() == "undefined" ? 0 : parseInt($("#ubication_ProvinciaId").val());
                                    $.appGlobales.actualplace.idCiudad = $("#ubication_CiudadId").val() == "undefined" ? 0 : parseInt($("#ubication_CiudadId").val());
                                    $.appGlobales.actualplace.idSector = $("#ubication_SectorId").val() == "undefined" ? 0 : parseInt($("#ubication_SectorId").val());
                                    $.appGlobales.actualplace.idLugar = $("#ubication_LugarId").val() == "undefined" ? 0 : parseInt($("#ubication_LugarId").val());

                                    $.appGlobales.actualplace.Pais = $("#ubication_PaisId").val() == "undefined" ? "" : $("#ubication_PaisId option[value=" + $("#ubication_PaisId").val() + "]").text();
                                    $.appGlobales.actualplace.Provincia = $("#ubication_ProvinciaId").val() == "undefined" ? "" : $("#ubication_ProvinciaId option[value=" + $("#ubication_ProvinciaId").val() + "]").text();
                                    $.appGlobales.actualplace.Ciudad = $("#ubication_CiudadId").val() == "undefined" ? "" : $("#ubication_CiudadId option[value=" + $("#ubication_CiudadId").val() + "]").text();
                                    $.appGlobales.actualplace.Sector = $("#ubication_SectorId").val() == "undefined" ? "" : $("#ubication_SectorId option[value=" + $("#ubication_SectorId").val() + "]").text();
                                    $.appGlobales.actualplace.Lugar = $("#ubication_LugarId").val() == "undefined" ? "" : $("#ubication_LugarId option[value=" + $("#ubication_LugarId").val() + "]").text();

                                    $.appGlobales.eliminaDialogVirtual(ui);

                                    if (fn != null) {
                                        var res = fn($.appGlobales.actualplace);
                                    }
                                });

                           
                                
                                return $.appGlobales.actualplace;

                            },
                            close: function () {

                            },
                            btnClose: "#ubication_content #closeDialog",
                        });
                        
                        inicia();
                        
                    },
                    error: function(xhr, status) {
                        alert(xhr.responseText);
                    },
                    complete: function(xhr, status) {

                    }
                });

                function inicia() {
                   
                    $("#ubication_Aceptar").show();

                    var $paisDropDown = $("#ubication_PaisId");
                    
                    if (required.substr(1, 1) == "1") {
                        $("#ubication_provinciaSection").show();
                    } else {
                        $("#ubication_provinciaSection").hide();
                    }
                    if (required.substr(2, 1) == "1") {
                        $("#ubication_ciudadSection").show();
                    } else {
                        $("#ubication_ciudadSection").hide();
                    }
                    if (required.substr(3, 1) == "1") {
                        $("#ubication_SectorSection").show();
                    } else {
                        $("#ubication_SectorSection").hide();
                    }

                    if (required.substr(4, 1) == "1") {
                        $("#ubication_LugarSection").show();
                    } else {
                        $("#ubication_LugarSection").hide();
                    }

                    loadProvincias();

                    if (required.length >= 2) {
                        $paisDropDown.change(function() {
                            loadProvincias();
                        });
                    }
                }

                function loadProvincias() {

                    var $paisDropDown = $("#ubication_PaisId");

                    $.ajax({
                        url: $.HostUrl() + "Provincia/GetDropProvincias",
                        data: { "idPais": $paisDropDown.val(), id: "ubication_ProvinciaId", selectedText: "Todas" },
                        type: 'POST',
                        dataType: 'html',
                        cache: false,
                        success: function(response) {
                            $("#ubication_provinciasDiv").html(response);

                            if ($.appGlobales.actualplace.idProvincia != "0") {
                                $("#ubication_ProvinciaId option[value=" + $.appGlobales.actualplace.idProvincia + "]").attr("selected", "selected");
                            }

                            var $provinciaDropDown = $("#ubication_ProvinciaId");

                            if (required.substr(2, 1) == "1") {
                                loadCiudades($provinciaDropDown.val());
                                $provinciaDropDown.change(function() {
                                    loadCiudades($(this).val());
                                });
                            }
                        },
                        error: function(xhr, status) {
                            alert(xhr.responseText);
                        },
                        complete: function(xhr, status) {

                        }
                    });
                }

                function loadCiudades(provId) {
                    var $paisDropDown = $("#ubication_PaisId");

                    $.ajax({
                        url: $.HostUrl() + "Ciudad/GetDropCiudades",
                        data: { "idPais": $paisDropDown.val(), "idProvincia": provId, id: "ubication_CiudadId" },
                        type: 'GET',
                        dataType: 'html',
                        cache: false,
                        success: function(response) {
                            $("#ubication_ciudadesDiv").empty().html(response);
                            
                            if (htmlTodas != null && htmlTodas != "") {
                                $("#ubication_CiudadId").prepend(htmlTodas);
                            }

                            //if ($.appGlobales.actualplace.idCiudad    != "0") {
                            //    $("#ubication_CiudadId option[value=" + $.appGlobales.actualplace.idCiudad + "]").attr("selected", "selected");
                            //}

                            $("#ubication_ciudadSection").show();

                            var $ciudadDropDown = $("#ubication_CiudadId");

                            if (required.substr(3, 1) == "1") {
                                loadSectores($ciudadDropDown.val());
                                $ciudadDropDown.change(function() {

                                    loadSectores($(this).val());
                                });
                            }
                        },
                        error: function(xhr, status) {
                            alert(xhr.responseText);
                        },
                        complete: function(xhr, status) {

                        }
                    });
                }

                function loadSectores(idCiudad) {
                    var $ciudadDropDown = $("#ubication_CiudadId");

                    $.ajax({
                        url: $.HostUrl() + "Sector/GetDropSectores",
                        data: { "idCiudad": $ciudadDropDown.val(), id: "ubication_SectorId", selectedText: "Todos" },
                        type: 'POST',
                        dataType: 'html',
                        cache: false,
                        success: function(response) {
                            $("#ubication_sectorDiv").empty()
                                .html(response);

                            if ($.appGlobales.actualplace.idSector != "0") {
                                $("#ubication_SectorId option[value=" + $.appGlobales.actualplace.idSector + "]").attr("selected", "selected");
                            }

                            $("#ubication_SectorSection").show();

                            var $sectorDropDown = $("#ubication_SectorId");

                            if (required.substr(4, 1) == "1") {
                                loadLugares($sectorDropDown.val());
                                $sectorDropDown.change(function() {
                                    loadLugares($(this).val());
                                });
                            }
                        },
                        error: function(xhr, status) {
                            alert(xhr.responseText);
                        },
                        complete: function(xhr, status) {

                        }
                    });
                }

                function loadLugares(idCiudad) {
                    var $ciudadDropDown = $("#ubication_CiudadId");
                    var $sectorDropDown = $("#ubication_SectorId");

                    $.ajax({
                        url: $.HostUrl() + "Lugar/GetDropLugares",
                        data: { "idCiudad": $ciudadDropDown.val(), idSector: $sectorDropDown.val(), id: "ubication_LugarId" },
                        type: 'POST',
                        dataType: 'html',
                        cache: false,
                        success: function(response) {
                            $("#ubication_lugarDiv").empty()
                                .html(response);

                            if ($.appGlobales.actualplace.idLugar != "0") {
                                $("#ubication_LugarId option[value=" + $.appGlobales.actualplace.idLugar + "]").attr("selected", "selected");
                            }

                            $("#ubication_LugarSection").show();
                        },
                        error: function(xhr, status) {
                            alert(xhr.responseText);
                        },
                        complete: function(xhr, status) {

                        }
                    });
                }
            },
            closeLastDialog: function() {
                $.appGlobales.stack[$.appGlobales.stack.length - 1].dialog("close");
                
            },
            resizeLastDialog:function (width, height) {
                $.appGlobales.stack[$.appGlobales.stack.length - 1].dialog("option", "height", height);
                $.appGlobales.stack[$.appGlobales.stack.length - 1].dialog("option", "width", width);
            },
            showZonaReportes: function (idUsuario, tipoReporte, sinEncabezado) {
                var usuario = idUsuario == "undefined" ? 0 : idUsuario;
                var tiporeporte = tipoReporte == "undefined" ? 0 : tipoReporte;
                $.ajax({
                    url: $.HostUrl() + "Reportes/GetReportes",
                    type: 'GET',
                    data: { tipoReporte: tiporeporte, idUsuario: usuario, sinEncabezado: sinEncabezado },
                    dataType: 'json',
                    cache: false,
                    success: function (response) {
                       
                        $("#zonaReportes").html(response.Xhtml);
                        $("#zonaReportes #dataReportes").html(response.Parametros["elementos"]);
                        var footer = response.JsonData["paginado"];
                        $.reportes.setFooter(footer);
                        $.reportes.setElementsEvent(tiporeporte );
                        $("#zonaReportes").css("display", "block");
                        $("#zonaReportes").dialog({
                            autoOpen: true,
                            height: 'auto',
                            width: 'auto',
                            position: ['center', 'center'],
                            modal: false,
                            resizable: false,
                            show: 'slide',
                            cache: false,
                            buttons: {},
                            title: "Mis Reportes",
                            close: function () {
                                $("#zonaReportes").html("");
                                $.appGlobales.clearIntervals();
                            }
                        });
                      
                        
                        $.appGlobales.stackRefrescamiento.push(setInterval("$.reportes.search()"  , 15000));

                    },
                    error: function (xhr, status) {
                        alert(xhr.responseText);
                    },
                    complete: function (xhr, status) {

                    }
                });
            },
            hideZonaReportes: function () {

                ("#zonaReportes").html();
                ("#zonaReportes").css("display", "none");
            },
            showZonaCorreos: function (idUsuario) {
                var usuario = idUsuario == "undefined" ? 0 : idUsuario;
                $.ajax({
                    url: $.HostUrl() + "Correos/GetCorreos",
                    type: 'GET',
                    data: { idUsuario: usuario },
                    dataType: 'json',
                    cache: false,
                    success: function (response) {
                        
                        $("#zonaCorreos").html(response.Xhtml);
                        $("#zonaCorreos #dataCorreos").html(response.Parametros["elementos"]);
                        var footer = response.JsonData["paginado"];
                        $.correos.setFooter(footer);
                        $.correos.setElementsEvent();
                        $("#zonaCorreos").css("display", "block");
                        $("#zonaCorreos").dialog({
                            autoOpen: true,
                            height: 'auto',
                            width: 'auto',
                            position: ['center', 'center'],
                            modal: false,
                            resizable: false,
                            show: 'slide',
                            cache: false,
                            buttons: {},
                            title: "Mis Correos",
                            close: function () {
                                $("#zonaCorreos").html("");
                                $.appGlobales.clearIntervals();
                            }
                        });

                        $.appGlobales.stackRefrescamiento.push(setInterval("$.correos.search()", 15000));
                        

                    },
                    error: function (xhr, status) {
                        alert(xhr.responseText);
                    },
                    complete: function (xhr, status) {

                    }
                });
            },
            hideZonaCorreos: function () {

                ("#zonaCorreos").html();
                ("#zonaCorreos").css("display", "none");
            },
            testInformes: function () {
                $.ajax({
                    url: $.HostUrl() + "Reportes/TestInformes",
                    type: 'GET',
                    data: { },
                    dataType: 'json',
                    cache: false,
                    success: function (response) {
                        $.appGlobales.showZonaReportes();

                    },
                    error: function (xhr, status) {
                        alert(xhr.responseText);
                    },
                    complete: function (xhr, status) {

                    }
                });
            },
            clearIntervals: function() {
                $.each($.appGlobales.stackRefrescamiento, function(i, item) {
                    clearInterval(item);
                });
                $.appGlobales.stackRefrescamiento = new Array();
            },
            CreateMenuFlotante: function (items, menuName) {
             
                var data = items;
                $.ajax({
                    url: $.HostUrl() + "Home/CreateMenuFlotante",
                    data: JSON.stringify({ "opciones": data }),
                    type: 'POST',
                    dataType: 'json',
                    cache: false,
                    contentType: "application/json; charset=utf-8",  // OJO IMPORTANTE MVC4
                    success: function (response) {
                     
                        $("#menuFlotante").html(response.Xhtml);


                        $("[id^=cu_]").unbind("click");
                        $("[id^=cu_]").bind("click", function (e) {

                            e.preventDefault();
                            
                            var id = $(this).attr("id");
                            var key = id.split('_')[1];
                        
                            var a = _($.appGlobales.eventos)
                             .find(function (t) { return t.name == menuName + "OpcionSeleccionada"; });
                            if (a != null) {
                                a.handler.fire({ type: menuName + "OpcionSeleccionada", key: key });
                            }

                           
                        });

                    },
                    error: function (xhr, status) {
                       
                        alert('Disculpe, existió un problema');
                    },
                    complete: function (xhr, status) {

                    },
                });
                return true;
            },
            ShowMenuFlotante: function (x, y) {
                
                $("#menuFlotante").css("left", x);
                $("#menuFlotante").css("top", y);
                $("#menuFlotante").show("slide");
            },
            HideMenuFlotante: function () {
                $("#menuFlotante").hide();
            },
         
            AdicionaHandler: function (evento, fn) {
                var a = _($.appGlobales.eventos)
                        .find(function (t) { return t.name == evento; });
                if (a == null) {
                    var ev = new EventTarget();
                    ev.addHandler(evento, fn);
                    $.appGlobales.eventos.push({ name: evento, handler: ev });
                    return ev;
                }
                return a;
                
                
               
                
            },
            creaDialogVirtual: function (options) {

                var idmod = "";
                var divmod = null;
                $.appGlobales.contadorGuid = $.appGlobales.contadorGuid + 1;
                var zindex = 9000 + $.appGlobales.contadorGuid;


                if (options != undefined) {

                    if (options.modal != undefined && options.modal == true) {

                        idmod = "#modal_" + $.appGlobales.contadorGuid;
                        var mod = $("#areaDialogos #aqui").before("<div id='modal_" + $.appGlobales.contadorGuid + "' class='ui-widget-overlay'></div>");
                        divmod = $("#areaDialogos " + idmod);

                        var wd = $(document).width() + "px";
                        var ht = $(document).height() + "px";

                        $(divmod).css("width", wd);
                        $(divmod).css("height", ht);
                        $(divmod).css("z-index", zindex);

                        $.appGlobales.contadorGuid = $.appGlobales.contadorGuid + 1;
                        zindex = 9000 + $.appGlobales.contadorGuid;

                        $(window).on("resize", function () {
                            wd = $(document).width() + "px";
                            ht = $(document).height() + "px";

                            $(divmod).css("width", wd);
                            $(divmod).css("height", ht);
                        });
                    }

                    var tar = $("#areaDialogos #aqui").before("<div id='wdialog_" + $.appGlobales.contadorGuid + "'  style='display: none; background-color: white' class='ui-widget-content ui-corner-all content-lais'></div>");
                    var div = $("#wdialog_" + $.appGlobales.contadorGuid);
                    var tmp = $("#areaTemporal");

                    var withdModal = 400;
                    var top = 0;
                    var left = 0;
                    if (options.withdModal != undefined) {
                        withdModal = options.withdModal;
                    } else {
                        if (options.html != undefined) {
                            $(tmp).html("");
                            $(tmp).html(options.html);
                            //$(div).html(options.html);
                            withdModal = $(div).width();
                        }
                    }

                    if (options.top != undefined) {
                        top = options.top;
                    } else {
                        var h = $(tmp).height();
                        // var h = $(div).height();
                        if ($(window).height() > h) {
                           // top = (parseInt($(window).height()) / 2) - (h / 2) + window.pageYOffset;
                            top = (parseInt($(window).height()) -h)  / 4 + window.pageYOffset;
                        } else {
                            top = window.pageYOffset;
                        }
                    }

                    if (options.left != undefined) {
                        left = options.left;
                    } else {
                        var w = $(tmp).width();

                        if ($(window).width() > w) {
                            left = (parseInt($(window).width()) / 2) - (w / 2) + window.pageXOffset;
                        } else {
                            left = window.pageXOffset;
                        }
                    }


                    //  left = (parseInt($("body").width()) / 2) - (withdModal / 2);



                    $(div).css("position", "absolute");
                    $(div).css("z-index", zindex);
                    $(div).css("display", "block");
                    $(div).css("left", left + "px");
                    $(div).css("top", top + "px");

                    if (options.html != undefined) {
                        $(tmp).html("");
                        $(div).append(options.html);

                    }
                    if (options.left != undefined) {
                        $(div).css("left", options.left);
                    }
                    if (options.top != undefined) {
                        $(div).css("top", options.top);
                    }

                    if (options.open != undefined) {
                        options.open($.appGlobales.contadorGuid);
                    }

                    if (options.btnClose != undefined) {
                        var closeFocalizado = "#wdialog_" + $.appGlobales.contadorGuid + " " + options.btnClose;
                        $(closeFocalizado).off("click");
                        $(closeFocalizado).on("click", function (event) {

                            if (options.close != undefined) {
                                options.close(event);
                            }

                            $(div).detach();
                            $(divmod).detach();


                        });
                    }
                    if (options.arrastraDesde != undefined ) {
                        $(div).draggable({ handle: options.arrastraDesde });
                    } else {
                        $(div).draggable();
                    }
                  
                    return $(div)[0];
                }
                return null;
            },
         
            eliminaDialogVirtual: function (dialogoId) {
                var idMod = parseInt(dialogoId) - 1;

                var div = $("#wdialog_" + dialogoId);
                var divmod = $("#modal_" + idMod);

                $(div).detach();
                $(divmod).detach();

            },
            showError: function (ventana) {

                if (ventana != undefined) {
                    $.appGlobales.creaDialogVirtual({
                        html: ventana,
                        modal: true,
                        btnClose: "#toolbarDialog #closeDialog",
                    });
                }

            },
            addManipulador: function (evento) {
                var a = _($.appGlobales.eventosArray)
                    .find(function (t) { return t.key == evento.key; });
                if (a == null) {
                    $.appGlobales.eventosArray.push(evento);
                } else {
                    a.eventos = evento.eventos;
                    a.state = evento.state;
                }

            },
            removeManipulador: function (manipulador) {

                var i = 0;
                _($.appGlobales.eventosArray).find(function (t) {
                    if (t.key == manipulador.key) {
                        $.appGlobales.eventosArray.splice(i, 1);
                        return;
                    }
                    i++;
                });

            },
            clearHandlers: function (manipulador, eventName) {

                if (manipulador != undefined && manipulador.eventos.handlers[eventName] != undefined) {
                    manipulador.eventos.handlers[eventName].splice(0);
                }
            },
            refreshGetOption: function (options) {
                //var options = { url: "", data: {}, target: "" };
                if (options != null && options != undefined) {

                    var url = options.url;
                    var data = options.data;
                    var target = options.target;

                    $.ajax({
                        url: $.HostUrl() + url,
                        data: data,
                        type: 'GET',
                        dataType: 'json',
                        cache: false,
                        success: function (response) {

                            $(target).html(response.Xhtml);

                        },
                        error: function (xhr, status) {
                            alert(xhr.responseText);
                        },
                        complete: function (xhr, status) {

                        }
                    });
                }

            },
            creaMensajeAlerta: function (alert, fn) {
                var estado = false;

                $.ajax({
                    url: $.HostUrl() + "Home/CreateMessageAlert",
                    data: { message: alert },
                    type: 'GET',
                    dataType: 'json',
                    cache: false,
                    async: false,
                    success: function (response) {

                        if (response.Result != "ERROR") {
                            $.appGlobales.creaDialogVirtual({
                                html: response.Xhtml,
                                modal: true,
                                open: function (ui) {

                                    var btonOk = $("#messageAlert #alertOk");
                                    var btonCancel = $("#messageAlert #alertCancel");

                                    $(btonOk).button({
                                        text: true
                                    });
                                    $(btonCancel).button({
                                        text: true
                                    });

                                    $("#messageAlert #alertCloseDialog").button({
                                        text: false,
                                        icons: {
                                            primary: "ui-icon-close"
                                        }
                                    });



                                },
                                close: function (event) {
                                  
                                    if (event.currentTarget.id == "alertOk") {
                                        estado = true;
                                    }

                                    fn(estado);
                                },
                                btnClose: "[id^=alert]",
                            });
                        } else {
                            $.appGlobales.showError(response.Xhtml);
                        }
                    },
                    error: function (xhr, status) {
                        alert(xhr.responseText);
                    },
                    complete: function (xhr, status) {
                    }
                });

                //debugger;
                return null;
            },
            creaMenuDesplegable: function (options, fn) {
                var saleEn = $(window).width() + "px";
                var html = "";
                var top = undefined;
                if (options.top != undefined) top = options.top;
                var direction = "right";
                var tiempo = 500;
                if (options.direction != undefined) {
                    direction = options.direction;
                    
                }
                if (options.tiempo != undefined) {
                    tiempo = options.tiempo;

                }
                if (direction != "right") {
                    saleEn = "5px";
                }
                if (options.html != undefined) {
                    html = options.html;
                }

                $.appGlobales.creaDialogVirtual({
                    html: html,
                    modal: true,
                    left: saleEn,
                    top: top,
                    arrastraDesde: "#ninguno",
                    open: function (ui) {
                        var uia = parseInt(ui) - 1;
                        $("#wdialog_" + ui).css("display", "none");
                        var ancho = parseInt($("#wdialog_" + ui).css("width"));
                        if (direction == "right") {
                            saleEn = ($(window).width() - ancho - 20) + "px";
                        }
                       
                        $("#wdialog_" + ui).css("left", saleEn);

                        $("#wdialog_" + ui).toggle("slide", { direction: direction }, tiempo);
                     
                        $("#modal_" +uia).off("click");
                        $("#modal_" +uia).on("click", function () {
                          
                            $("#wdialog_" + ui).toggle("slide", { direction: direction }, tiempo, function () {
                               
                                $("#wdialog_" + ui).detach();
                                $("#modal_" + uia).detach();
                            });
                       
                        });
                        if (fn!=undefined) {
                            fn({guid:ui, direction:direction, tiempo:tiempo});
                        }

                    },
                    close: function (event) {

                        //if (event.currentTarget.id == "alertOk") {
                        //    estado = true;
                        //}

                        //fn(estado);
                    },
                    btnClose: "[id^=alert]",
                });
            },
            
            toClockFormat : function (segundos) {
                var min = "00";
                var seg = "00";
                var m = parseInt(segundos / 60);
                var s = segundos - m * 60;
                min = m.toString();
                seg = s.toString();
              
                if (m.toString().length== 1 ) {
                    min = "0" + m.toString();
                }
                if (s.toString().length== 1) {
                    seg = "0" + s.toString();
                }
                return min + ":" + seg;
            }
            
        };
        $.appGlobales.touch = Modernizr.touch;        
    },
    
    LaiMessageBox: function() {

        // fid: Nombre de la clase (formulario id), a través de la cual
        //      se formara el nombre del elemento dentro del cual se creara el dialog.
        // title: Título del dialogo de confirmacion de eliminacion.
        // text: Texto del dialogo de confirmacion de eliminacion.
        // id: Id del dialog que se va a crear.
        // parent: Contenedor donde se creará el div del dialog.
        // flag: Bandera que determinará si al presionar el botón aceptar se cerrará y destruirá el dialog.
        // Aceptar y Cancelar son las funciones que se ejecutarán del otro lado cuando 
        // sepresionen los botones aceptar o cancelar respectivamente.

        var args = arguments[0] || { };
        var fid = args.fid;
        var title = (args.title == "undefined" || args.title == null) ? "Información" : args.title;
        var text = (args.text == "undefined" || args.text == null) ? "Se eliminará el elemento de forma permanente. ¿Está seguro de eliminar?" : args.text;
        var id = (args.id == "undefined" || args.id == null) ? "delete_" + fid : args.id;
        var parent = (args.parent == "undefined" || args.parent == null) ? "#form_" + fid + " #dialogs" : args.parent;
        var flag = (args.flag == "undefined" || args.flag == null) ? true : args.flag;

        var Aceptar = args.Aceptar;
        var Cancelar = args.Cancelar;


        var $tar = $("#" + parent);
        $tar.append("<div id='" + id + "' title='" + title + "'> <p><span class='ui-icon ui-icon-alert' style='float:left; margin:0 7px 20px 0;'></span> " + text + " </p></div>");
        var $dialo = $("#" + id);
        $dialo.dialog({
            autoOpen: true,
            resizable: false,
            height: 140,
            modal: true,
            buttons: {
                "Aceptar": function() {
                    $dialo.bind('Aceptar', Aceptar);
                    $dialo.trigger('Aceptar');
                    if (flag) {
                        $dialo.dialog("close");
                        $dialo.remove();
                    }
                },
                "Cancelar": function() {
                    $dialo.bind('Cancelar', Cancelar);
                    $dialo.trigger('Cancelar');
                    $dialo.dialog("close");
                    $dialo.remove();
                }
            }
        });
        $dialo.dialog("show");
    },

    StartLayout: function() {
        $("#side-menu [id^=_R]").on("click", function (item) {
            if ($(item.target).hasClass("hijo")) {
                $.LoadContent($.HostUrl() + "Home/ResponseMenu", item.currentTarget.id);
            }
        });
    },

    HostUrl: function() {
        return $("#host").val();
    },

    Edit: function() {
        // elemtId: Id del elemento a editar.
        // page: Pagina actual del paginado.
        // action: Accion del controlador para editarm el elemto.
        // id: El id del dialog que se va a crear.
        // fid: Nombre de la clase, a través de la cual
        //      se formara el nombre del elemento dentro del cual se creara el dialog.
        // title: Titulo del dialog.

        var args = arguments[0] || { };
        var fid = args.fid;
        var action = (args.action == "undefined" || args.action == null) ? fid + "/Edit" : args.action;
        var id = (args.id == "undefined" || args.id == null) ? "edit_" + fid : args.id;
        var elemtId = (args.elemtId == "undefined" || args.elemtId == null) ? 0 : args.elemtId;
        var page = (args.page == "undefined" || args.page == null) ? 1 : args.page;
        var titleEditClass = (args.titleEditClass == "undefined" || args.titleEditClass == null) ? fid : args.titleEditClass;

        var title = "Editar " + titleEditClass;
        if (elemtId == 0) {
            title = 'Crear ' + titleEditClass;
        }
        var $dialogo = $.CreateDivDialog(id, fid, title);
        $.ajax({
            url: $.HostUrl() + action,
            data: { id: elemtId, page: page },
            type: 'GET',
            dataType: 'html',
            cache: false,
            success: function(response) {
                //var $dialogo = $("#form_" + fid + " #dialogs #" + id);
                $dialogo.html(response)
                    .dialog('open');
            },
            error: function(xhr, status) {
                alert(xhr.responseText);
            },
            complete: function(xhr, status) {
                return false;
            }
        });
    },
    
    EditJson: function () {
        // elemtId: Id del elemento a editar.
        // page: Pagina actual del paginado.
        // action: Accion del controlador para editarm el elemto.
        // id: El id del dialog que se va a crear.
        // fid: Nombre de la clase, a través de la cual
        //      se formara el nombre del elemento dentro del cual se creara el dialog.
        // title: Titulo del dialog.

        var args = arguments[0] || {};
        var fid = args.fid;
        var action = (args.action == "undefined" || args.action == null) ? fid + "/Edit" : args.action;
        var id = (args.id == "undefined" || args.id == null) ? "edit_" + fid : args.id;
        var elemtId = (args.elemtId == "undefined" || args.elemtId == null) ? 0 : args.elemtId;
        var page = (args.page == "undefined" || args.page == null) ? 1 : args.page;
        var titleEditClass = (args.titleEditClass == "undefined" || args.titleEditClass == null) ? fid : args.titleEditClass;

        var title = "Editar " + titleEditClass;
        if (elemtId == 0) {
            title = 'Crear ' + titleEditClass;
        }

        $.ajax({
            url: $.HostUrl() + action,
            data: { id: elemtId, page: page },
            type: 'GET',
            dataType: 'json',
            cache: false,
            success: function (response) {

                var d = $.CreateDialogModal(title);
                d.html(response.Xhtml);
                d.dialog('open');

            },
            error: function (xhr, status) {
                alert(xhr.responseText);
            },
            complete: function (xhr, status) {
                return false;
            }
        });
    },

    EditAutomatico: function() {
        // elemtId: Id del elemento a editar.
        // page: Pagina actual del paginado.
        // action: Accion del controlador para editarm el elemto.
        // id: El id del dialog que se va a crear.
        // fid: Nombre de la clase, a través de la cual
        //      se formara el nombre del elemento dentro del cual se creara el dialog.
        // title: Titulo del dialog.

        var args = arguments[0] || { };
        var fid = args.fid;
        var action = (args.action == "undefined" || args.action == null) ? fid + "/Edit" : args.action;
        var id = (args.id == "undefined" || args.id == null) ? "edit_" + fid : args.id;
        var elemtId = (args.elemtId == "undefined" || args.elemtId == null) ? 0 : args.elemtId;
        var page = (args.page == "undefined" || args.page == null) ? 1 : args.page;
        var titleEditClass = (args.titleEditClass == "undefined" || args.titleEditClass == null) ? fid : args.titleEditClass;

        var title = "Editar " + titleEditClass;
        if (elemtId == 0) {
            title = 'Crear ' + titleEditClass;
        }
        var $dialogo = $.CreateDivDialog(id, fid, title);
        $.ajax({
            url: $.HostUrl() + action,
            data: { id: elemtId, page: page },
            type: 'GET',
            dataType: 'html',
            cache: false,
            success: function(response) {
                var d = $.CreateDialogModal(title);
                d.html(response);
                d.dialog("open");
                return d;
            },
            error: function(xhr, status) {
                alert(xhr.responseText);
            },
            complete: function(xhr, status) {

            }
        });
    },

    EditPOST: function(id, actionEdit, fid) {
        // id: Id del dialog de Editar.
        // actionEdit: Acción del controlador que editará el alemento.
        // fid: Nombre de la clase.

        var $save = $("#" + id + " #saveEdit");
        var $dialog = $("#" + id);

        $save.button({
            text: true
        });

        $save.click(function() {
            $.ajax({
                url: $.HostUrl() + actionEdit,
                data: $("[role=edit]").serialize(),
                type: 'POST',
                dataType: 'html',
                cache: false,
                success: function(response) {
                    $dialog.empty();
                    if (response.indexOf("SAVED") > 0) {
                        $dialog.dialog('close');
                        $.RefreshGrid({ fid: fid, page: $("#form_" + fid + " #currentPage").val() });
                    } else {
                        if (response.indexOf("CREATED") > 0) {
                            $dialog.dialog('close');
                            $.RefreshGrid({ fid: fid, page: $("#form_" + fid + " #currentPage").val() });
                            //$.RefreshGrid({ fid: fid, filtered: false });
                        } else {
                            $dialog.append(response);
                            $dialog.dialog('open');
                        }
                    }

                },
                error: function(xhr, status) {
                    alert(xhr.responseText);
                },
                complete: function(xhr, status) {

                }
            });
        });
    },

    StartUbication: function() {
        // fid: Nombre de la clase (formulario id), a través de la cual
        //      se formara el nombre del elemento dentro del cual se creara el dialog.
        // parentDialog: Id del contenedor donde se creará el div padre del dialog.
        // functionEvent: Función que se ejecutará cuando se presione aceptar. 
        //                La funcción debe recibir como parámetro: (e, json con los id de cada una de las ubicaciones, cad 
        //                con la concatenación de los textos de las ubicaciones )
        // required: Ubicaciones requeridas.
        // selectedPais: Id del país que se seleccionará por defecto.

        var args = arguments[0] || { };
        var fid = args.fid;
        var parentDialog = (args.parentDialog == "undefined" || args.parentDialog == null) ? "form_" + fid : args.parentDialog;
        //var parentDialog = (args.parentDialog == "undefined" || args.parentDialog == null) ? "form_" + fid : args.parentDialog;
        var functionEvent = args.functionEvent;
        var required = (args.required == "undefined" || args.required == null) ? "10100" : args.required;
        var selectedPais = (args.selectedPais == "undefined" || args.selectedPais == null || args.selectedPais == "") ? "" : args.selectedPais;
        var selectedProvincia = (args.selectedProvincia == "undefined" || args.selectedProvincia == null || args.selectedProvincia == "") ? "" : args.selectedProvincia;
        var selectedCiudad = (args.selectedCiudad == "undefined" || args.selectedCiudad == null || args.selectedCiudad == "") ? "" : args.selectedCiudad;
        var selectedSector = (args.selectedSector == "undefined" || args.selectedSector == null || args.selectedSector == "") ? "" : args.selectedSector;
        var selectedLugar = (args.selectedLugar == "undefined" || args.selectedLugar == null || args.selectedLugar == "") ? "" : args.selectedLugar;

        var title = "Seleccionar Ubicación";
        var idDialog = "ubication_" + fid;

        var $dialog = $.CreateDivDialog(idDialog, fid, title);
        $dialog.dialog({
            close: function() {
                $dialog.remove();
            }
        });
        var pId = (selectedPais != "") ? selectedPais : 0;
        $.ajax({
            url: $.HostUrl() + fid + "/Ubication",
            type: 'POST',
            data: { PaisId: pId },
            dataType: 'html',
            cache: false,
            success: function(response) {
                $dialog.html(response)
                    .dialog('open');
                $dialog.dialog({
                    buttons: {
                        "Aceptar": function() {
                            $("#ubication_validations").empty();
                            var p = Accept();
                            if (p) {
                                $(this).dialog("close");
                            }
                        },
                        "Cancelar": function() {
                            $(this).dialog("close");
                        }
                    }
                });
                Success();
            },
            error: function(xhr, status) {
                alert(xhr.responseText);
            },
            complete: function(xhr, status) {

            }
        });

        function Accept() {
            var tempPais = $("#" + idDialog + " #ubication_PaisId").val();
            var pais = (tempPais == "undefined" || tempPais == null || tempPais == "" || tempPais == "foo") ? 0 : tempPais;

            var tempProvincia = $("#" + idDialog + " #ubication_ProvinciaId").val();
            var provincia = (tempProvincia == "undefined" || tempProvincia == null || tempProvincia == "" || tempProvincia == "foo") ? 0 : tempProvincia;

            var tempCiudad = $("#" + idDialog + " #ubication_CiudadId").val();
            var ciudad = (tempCiudad == "undefined" || tempCiudad == null || tempCiudad == "" || tempCiudad == "foo") ? 0 : tempCiudad;

            var tempSector = $("#" + idDialog + " #ubication_SectorId").val();
            var sector = (tempSector == "undefined" || tempSector == null || tempSector == "" || tempSector == "foo") ? 0 : tempSector;

            var tempLugar = $("#" + idDialog + " #ubication_LugarId").val();
            var lugar = (tempLugar == "undefined" || tempLugar == null || tempLugar == "" || tempLugar == "foo") ? 0 : tempLugar;


            var tempPaiscad = $("#" + idDialog + " #ubication_PaisId option[value='" + tempPais + "']").text();
            var paiscad = (tempPaiscad == "undefined" || tempPaiscad == null || tempPaiscad == "" || tempPaiscad == "foo" || tempPaiscad == "Todas" || tempPaiscad == "Todos" || tempPaiscad == "Seleccione") ? "Sin Definir" : tempPaiscad;

            var tempProvinciacad = $("#" + idDialog + " #ubication_ProvinciaId option[value='" + tempProvincia + "']").text();
            var provinciacad = (tempProvinciacad == "undefined" || tempProvinciacad == null || tempProvinciacad == "" || tempProvinciacad == "foo" || tempProvinciacad == "Todas" || tempProvinciacad == "Todos" || tempProvinciacad == "Seleccione") ? "Sin Definir" : tempProvinciacad;

            var tempCiudadcad = $("#" + idDialog + " #ubication_CiudadId option[value='" + tempCiudad + "']").text();
            var ciudadcad = (tempCiudadcad == "undefined" || tempCiudadcad == null || tempCiudadcad == "" || tempCiudadcad == "foo" || tempCiudadcad == "Todas" || tempCiudadcad == "Todos" || tempCiudadcad == "Seleccione") ? "Sin Definir" : tempCiudadcad;

            var tempSectorcad = $("#" + idDialog + " #ubication_SectorId option[value='" + tempSector + "']").text();
            var sectorcad = (tempSectorcad == "undefined" || tempSectorcad == null || tempSectorcad == "" || tempSectorcad == "foo" || tempSectorcad == "Todas" || tempSectorcad == "Todos" || tempSectorcad == "Seleccione") ? "Sin Definir" : tempSectorcad;

            var tempLugarcad = $("#" + idDialog + " #ubication_LugarId option[value='" + tempLugar + "']").text();
            var lugarcad = (tempLugarcad == "undefined" || tempLugarcad == null || tempLugarcad == "" || tempLugarcad == "foo" || tempLugarcad == "Todas" || tempLugarcad == "Todos" || tempLugarcad == "Seleccione") ? "Sin Definir" : tempLugarcad;

            var flag = true;
            var $val = $("#ubication_validations");
            $val.empty();

            if (pais == 0 && required[0] == 1) {
                $val.append("<div>El país es requerido.</div>");
                flag = false;
            }
            if (provincia == 0 && required[1] == 1) {
                $val.append("<div>La provincia es requerida.</div>");
                flag = false;
            }
            if (ciudad == 0 && required[2] == 1) {
                $val.append("<div>La ciudad es requerida.</div>");
                flag = false;
            }
            if (sector == 0 && required[3] == 1) {
                $val.append("<div>El sector es requerido.</div>");
                flag = false;
            }
            if (lugar == 0 && required[4] == 1) {
                $val.append("<div>El lugar es requerido.</div>");
                flag = false;
            }

            var obj = jQuery.parseJSON('{"Pais":"' + pais + '", "Provincia":"' + provincia + '", "Ciudad":"' + ciudad + '", "Sector":"' + sector + '", "Lugar":"' + lugar + '", "PaisText":"' + paiscad + '", "ProvinciaText":"' + provinciacad + '", "CiudadText":"' + ciudadcad + '", "SectorText":"' + sectorcad + '", "LugarText":"' + lugarcad + '"}');

            var cad = paiscad;
            if (required.length >= 2 && required[1] == "1") {
                cad = provinciacad;
            } else {
                if (required.length >= 3 && required[2] == "1") {
                    cad = ciudadcad;
                }
            }

            if (required.length >= 4 && required[3] == "1") {
                cad = sectorcad;
            } else {
                if (required.length >= 5 && required[4] == "1") {
                    cad = lugarcad;
                }
            }

            if (flag) {
                $dialog.bind('Aceptar', functionEvent);
                $dialog.trigger('Aceptar', [obj, cad]);
            } else {
                $val.show("blind");
            }
            return flag;

        }

        function Success() {
            var $paisDropDown = $("#" + idDialog + " #ubication_PaisId");

            //            if (selectedPais != "") {
            //                $("#" + idDialog + " #ubication_PaisId option[value=" + selectedPais + "]").attr("selected", "selected");
            //            }

            //            if (required.length >= 1) {
            //                $("#" + idDialog + " #ubication_paisSection").show();
            //            }
            if (required.length >= 2) {
                $("#" + idDialog + " #ubication_provinciaSection").show();
            }
            if (required.length >= 3) {
                $("#" + idDialog + " #ubication_ciudadSection").show();
            }
            if (required.length >= 4) {
                $("#" + idDialog + " #ubication_SectorSection").show();
            }
            if (required.length >= 5) {
                $("#" + idDialog + " #ubication_LugarSection").show();
            }

            LoadProvincias();

            if (required.length >= 2) {
                $paisDropDown.change(function() {
                    LoadProvincias();
                });
            }
        }

        function LoadProvincias() {

            var $paisDropDown = $("#" + idDialog + " #ubication_PaisId");

            $.ajax({
                url: $.HostUrl() + "Provincia/GetDropProvincias",
                data: { "idPais": $paisDropDown.val(), id: "ubication_ProvinciaId", selectedText: "Todas" },
                type: 'POST',
                dataType: 'html',
                cache: false,
                success: function(response) {
                    $("#" + idDialog + " #ubication_provinciasDiv").html(response);

                    if (selectedProvincia != "") {
                        $("#" + idDialog + " #ubication_ProvinciaId option[value=" + selectedProvincia + "]").attr("selected", "selected");
                    }

                    var $provinciaDropDown = $("#" + idDialog + " #ubication_ProvinciaId");

                    if (required.length >= 3) {
                        LoadCiudades($provinciaDropDown.val());
                        $provinciaDropDown.change(function() {
                            LoadCiudades($(this).val());
                        });
                    }
                },
                error: function(xhr, status) {
                    alert(xhr.responseText);
                },
                complete: function(xhr, status) {

                }
            });
        }

        function LoadCiudades(provId) {
            var $paisDropDown = $("#" + idDialog + " #ubication_PaisId");

            $.ajax({
                url: $.HostUrl() + "Ciudad/GetDropCiudades",
                data: { "idPais": $paisDropDown.val(), "idProvincia": provId, id: "ubication_CiudadId" },
                type: 'GET',
                dataType: 'html',
                cache: false,
                success: function(response) {
                    $("#" + idDialog + " #ubication_ciudadesDiv").empty()
                        .html(response);

                    if (selectedCiudad != "") {
                        $("#" + idDialog + " #ubication_CiudadId option[value=" + selectedCiudad + "]").attr("selected", "selected");
                    }

                    $("#" + idDialog + " #ubication_ciudadSection").show();

                    var $ciudadDropDown = $("#" + idDialog + " #ubication_CiudadId");

                    if (required.length >= 4) {
                        LoadSectores($ciudadDropDown.val());
                        $ciudadDropDown.change(function() {
                            LoadSectores($(this).val());
                        });
                    }
                },
                error: function(xhr, status) {
                    alert(xhr.responseText);
                },
                complete: function(xhr, status) {

                }
            });
        }

        function LoadSectores(idCiudad) {
            var $ciudadDropDown = $("#" + idDialog + " #ubication_CiudadId");

            $.ajax({
                url: $.HostUrl() + "Sector/GetDropSectores",
                data: { "idCiudad": $ciudadDropDown.val(), id: "ubication_SectorId", selectedText: "Todos" },
                type: 'POST',
                dataType: 'html',
                cache: false,
                success: function(response) {
                    $("#" + idDialog + " #ubication_sectorDiv").empty()
                        .html(response);

                    if (selectedSector != "") {
                        $("#" + idDialog + " #ubication_SectorId option[value=" + selectedSector + "]").attr("selected", "selected");
                    }

                    $("#" + idDialog + " #ubication_SectorSection").show();

                    var $sectorDropDown = $("#" + idDialog + " #ubication_SectorId");

                    if (required.length >= 5) {
                        LoadLugares($sectorDropDown.val());
                        $sectorDropDown.change(function() {
                            LoadLugares($(this).val());
                        });
                    }
                },
                error: function(xhr, status) {
                    alert(xhr.responseText);
                },
                complete: function(xhr, status) {

                }
            });
        }

        function LoadLugares(idCiudad) {
            var $ciudadDropDown = $("#" + idDialog + " #ubication_CiudadId");
            var $sectorDropDown = $("#" + idDialog + " #ubication_SectorId");

            $.ajax({
                url: $.HostUrl() + "Lugar/GetDropLugares",
                data: { "idCiudad": $ciudadDropDown.val(), idSector: $sectorDropDown.val(), id: "ubication_LugarId" },
                type: 'POST',
                dataType: 'html',
                cache: false,
                success: function(response) {
                    $("#" + idDialog + " #ubication_lugarDiv").empty()
                        .html(response);

                    if (selectedLugar != "") {
                        $("#" + idDialog + " #ubication_LugarId option[value=" + selectedLugar + "]").attr("selected", "selected");
                    }

                    $("#" + idDialog + " #ubication_LugarSection").show();
                },
                error: function(xhr, status) {
                    alert(xhr.responseText);
                },
                complete: function(xhr, status) {

                }
            });
        }

    },

    AjaxDialogResponse: function() {
        // idDialog: El id del dialog que se va a crear.
        // fid: Nombre de la clase (formulario id), a través de la cual
        //      se formara el nombre del elemento dentro del cual se creara el dialog.
        // title: Titulo del dialog.
        // idElemet: Id del elemento 
        // action: Acción a la cual se hará la petición Ajax.

        var args = arguments[0] || { };
        var fid = args.fid;
        var idDialog = args.idDialog;
        var title = args.title;
        var action = args.action;
        var idElemet = args.idElemet;

        var $dialog = $.CreateDivDialog(idDialog, fid, title);
        $dialog.dialog({
            close: function() {
                $dialog.remove();
            }
        });

        $.ajax({
            url: $.HostUrl() + action,
            data: { "id": idElemet },
            type: 'POST',
            dataType: 'html',
            cache: false,
            success: function(response) {
                $dialog.html(response)
                    .dialog('open');
            },
            error: function(xhr, status) {
                alert(xhr.responseText);
            },
            complete: function(xhr, status) {

            }
        });
        return false;
    },
    
    AjaxDialogVirtualResponse: function () {
        // idDialog: El id del dialog que se va a crear.
        // fid: Nombre de la clase (formulario id), a través de la cual
        //      se formara el nombre del elemento dentro del cual se creara el dialog.
        // title: Titulo del dialog.
        // idElemet: Id del elemento 
        // action: Acción a la cual se hará la petición Ajax.

        var args = arguments[0] || {};
        var fid = args.fid;
        var idDialog = args.idDialog;
        var title = args.title;
        var action = args.action;
        var idElemet = args.idElemet;
        var target = args.target;
        //var $dialog = $.CreateDivDialog(idDialog, fid, title);
        //$dialog.dialog({
        //    close: function () {
        //        $dialog.remove();
        //    }
        //});

        $.ajax({
            url: $.HostUrl() + action,
            data: { "id": idElemet },
            type: 'POST',
            dataType: 'html',
            cache: false,
            success: function (response) {
               
                var dialog = $.appGlobales.creaDialogVirtual({
                    target: target,
                    html: response,
                    modal: false,
                    open: function (ui) {
                        //alert("open" + ui.id);
                    },
                    close: function () {
                        //alert("close");
                    },
                    //btnClose: "#closeAcomodacionMenu",
                });

                //$dialog.html(response)
                //    .dialog('open');
            },
            error: function (xhr, status) {
                alert(xhr.responseText);
            },
            complete: function (xhr, status) {

            }
        });
        return false;
    },

    CreateDivDialog: function(id, fid, title) {
        // id: El id del dialog que se va a crear.
        // fid: Nombre de la clase (formulario id), a través de la cual
        //      se formara el nombre del elemento dentro del cual se creara el dialog.
        // title: Titulo del dialog.
        var $tar = $("#form_" + fid + " #dialogs");
        $tar.append("<div id='" + id + "' class='ui-widget-content ui-helper-clearfix'></div>");
        var $dialo = $("#form_" + fid + " #dialogs #" + id);
        $dialo.dialog({
            autoOpen: false,
            height: 'auto',
            width: 'auto',
            position: ['center', 'center'],
            modal: true,
            resizable: false,
            show: 'slide',
            cache: false,
            buttons: { },
            title: title,
            close: function() {
                $dialo.remove();
            }
        });
        return $dialo;
    },

    DeleteRow: function() {
        // fid: Nombre de la clase (formulario id), a través de la cual
        //      se formara el nombre del elemento dentro del cual se creara el dialog.
        // title: Título del dialogo de confirmacion de eliminacion.
        // text: Texto del dialogo de confirmacion de eliminacion.
        // action: Accion del controlador encargada de eliminar los elementos.
        // ids: Ids de los elementos a eliminar separados por _.

        var args = arguments[0] || { };
        var fid = args.fid;
        var title = (args.title == "undefined" || args.title == null) ? "Información" : args.title;
        var text = (args.text == "undefined" || args.text == null) ? "Se eliminará el elemento de forma permanente. ¿Está seguro de eliminar?" : args.text;
        var action = (args.action == "undefined" || args.action == null) ? fid + "/DeleteRows" : args.action;
        var ids = (args.ids == "undefined" || args.ids == null) ? "" : args.ids;

        var $tar = $("#form_" + fid + " #dialogs");
        var $grid = $("#form_" + fid + " #grid");
        $tar.append("<div id='delete_" + fid + "' title='" + title + "'> <p><span class='ui-icon ui-icon-alert' style='float:left; margin:0 7px 20px 0;'></span> " + text + " </p></div>");
        var $dialo = $("#form_" + fid + " #dialogs #delete_" + fid);

        if (ids == "") {
            $.GetChkboxsMarcados(); //Hacer algo para poner esto generico tambien
            ids = $("#form_" + fid + " #idMarcados").val();
        }

        $dialo.dialog({
            autoOpen: true,
            resizable: false,
            height: 140,
            modal: true,
            buttons: {
                "Aceptar": function() {
                    $.ajax({
                        url: $.HostUrl() + action,
                        data: { "lischecks": ids },
                        type: 'POST',
                        dataType: 'html',
                        cache: false,
                        success: function(response) {
                            if (response.indexOf("--!=validar=!--") > 0) {
                                $grid.html(response);
                                $.ChangeStateButtonToolBar(fid, "sssdss");
                                $dialo.dialog("close");
                            } else {
                                $grid.html(response);
                                $.ChangeStateButtonToolBar(fid, "sssdss");
                                $dialo.dialog("close");
                            }
                        },
                        error: function(xhr, status) {

                            alert(xhr.responseText);
                        },
                        complete: function(xhr, status) {
                        }
                    });
                },
                "Cancelar": function() {
                    $dialo.dialog("close");
                }
            },
            close: function() {
                $dialo.remove();
            }
        });
    },

    CloseViews: function () {

        $.ajax({
            url: $.HostUrl() + "Home/Centro",
            data: { "id": 0 },
            type: 'GET',
            dataType: 'json',
            cache: false,
            success: function (response) {
                //alert(response["cmd"]);
                $("#content").html(response["codigohtml"]);
            },
            error: function (xhr, status) {

                alert(xhr.responseText);
            },
            complete: function (xhr, status) {
            }
        });
    },

    LoadPage: function (page, itemsPage, action, target, fn) {

        var url = $.HostUrl() + action;
        var $targetDiv = $("#" + target);

        $.ajax({
            url: url,
            data: { "id": 0, "page": page, "itemsPerPage": itemsPage },
            type: 'POST',
            dataType: 'html',
            cache: false,
            success: function (response) {
                $targetDiv.html(response);
                if (fn != null) {
                    fn();
                }
            },
            error: function (xhr, status) {
                alert(xhr.responseText);
            },
            complete: function (xhr, status) {

            }
        });

    },
    StartPaginado: function (fid, action, target, fn) {

        var $FP = $("#footer_" + fid + " #firstPage");
        var $NP = $("#footer_" + fid + " #nextPage");
        var $PP = $("#footer_" + fid + " #previousPage");
        var $LP = $("#footer_" + fid + " #lastPage");
        var $anyP = $("#footer_" + fid + " .pag");
        var $stateP = $("#footer_" + fid + " #paginationState");
        var $goP = $("#footer_" + fid + " #goPage");
        var $itsP = $("#footer_" + fid + " #itemsPerPage");


        $NP.button({
            text: false,
            icons: {
                primary: "ui-icon-seek-next"
            }
        });

        $PP.button({
            text: false,
            icons: {
                primary: "ui-icon-seek-prev"
            }
        });

        $FP.button({
            text: false,
            icons: {
                primary: "ui-icon-seek-first"
            }
        });

        $LP.button({
            text: false,
            icons: {
                primary: "ui-icon-seek-end"
            }
        });


        var bit = $stateP.val();
        var spl = null;
        if (bit != null) {
            spl = bit.split('');
            if (spl[0] == 0) {
                $FP.button({ disabled: true });
            } else {
                $FP.button({ disabled: false });
            }

            if (spl[1] == 0) {
                $PP.button({ disabled: true });
            } else {
                $PP.button({ disabled: false });
            }

            if (spl[2] == 0) {
                $NP.button({ disabled: true });
            } else {
                $NP.button({ disabled: false });
            }

            if (spl[3] == 0) {
                $LP.button({ disabled: true });
            } else {
                $LP.button({ disabled: false });
            }
        }

        $anyP.click(function (actual) {
            $.LoadPage($(actual.currentTarget).attr("title"), $itsP.val(), action, target, fn);
        });
        $goP.keypress(function (e) {

            if (e.keyCode == 13) {

                if (parseInt($goP.val()) > parseInt($LP.attr("title"))) $goP.val($LP.attr("title"));
                if (parseInt($goP.val()) < parseInt($FP.attr("title"))) $goP.val($FP.attr("title"));
                if (parseInt($goP.val()) == 0) $goP.val($FP.attr("title"));
                $.LoadPage($goP.val(), $itsP.val(), action, target, fn);
            }
        });
        $itsP.keypress(function (e) {

            if (e.keyCode == 13) {
                $.LoadPage(1, $itsP.val(), action, target, fn);
            }
        });



    },

    LoadContent: function(url, data) {
        $.ajax({
            url: url,
            data: { 'Key': data },
            type: 'GET',
            dataType: 'html',
            cache: false,
            success: function(response) {
                $("#main-content").html(response);
            },
            error: function(xhr, status) {
                alert("No se encuentra la página");
                alert(xhr.responseText);
            },
            complete: function(xhr, status) {
            }
        });

    },
    
    LoadFloatContent: function (url, data) {

        $.ajax({
            url: url,
            data: { 'Key': data },
            type: 'GET',
            dataType: 'html',
            cache: false,
            success: function (response) {

                var d = $.CreateDialogModal("", position);
                d.html(response);
                d.dialog("open");
                

            },
            error: function (xhr, status) {
                alert("No se encuentra la página");
                alert(xhr.responseText);
            },
            complete: function (xhr, status) {
            }

        });

    },

    LoadAjaxLoader: function() {

        $("#load").hide();

        $("#load").on("ajaxStart", function() {
            $(this).show("drop", { direction: "up" }, "100");
        }).on("ajaxStop", function() {
            $(this).hide("drop", { direction: "up" }, "100");
        });
        var actionKey = $("#urlActionKey").val();
        if (actionKey != "" && actionKey != null) {
            $.LoadContent($.HostUrl() + "Home/ResponseMenu", actionKey);
        }
    },

    ChangeStateButtonToolBar: function(fid, states) {
        //s = el mismo que tiene (no cambiarlo)
        //h = hide (oculto)
        //d = disabled (desabilitado)
        //e = enabled (habilitado)

        var st = states.split("");

        var $filter = $("#form_" + fid + " #toolbar #filter");
        var $search = $("#form_" + fid + " #toolbar #search");
        var $plus = $("#form_" + fid + " #toolbar #plus");
        var $delete = $("#form_" + fid + " #toolbar #delete");
        var $help = $("#form_" + fid + " #toolbar #help");
        var $close = $("#form_" + fid + " #toolbar #close");

        if (st[0] != "s") {
            if (st[0] == "h") {
                $filter.hide();
            } else {
                if (st[0] == "e") {
                    $filter.button({
                        disabled: false
                    });
                } else {
                    $filter.button({
                        disabled: true
                    });
                }
            }
        }

        if (st[1] != "s") {
            if (st[1] == "h") {
                $search.hide();
            } else {
                if (st[1] == "e") {
                    $search.button({
                        disabled: false
                    });
                } else {
                    $search.button({
                        disabled: true
                    });
                }
            }
        }

        if (st[2] != "s") {
            if (st[2] == "h") {
                $plus.hide();
            } else {
                if (st[2] == "e") {
                    $plus.button({
                        disabled: false
                    });
                } else {
                    $plus.button({
                        disabled: true
                    });
                }
            }
        }

        if (st[3] != "s") {
            if (st[3] == "h") {
                $delete.hide();
            } else {
                if (st[3] == "e") {
                    $delete.button({
                        disabled: false
                    });
                } else {
                    $delete.button({
                        disabled: true
                    });
                }
            }
        }

        $help.hide();  // Poner siempre oculto la ayuda hasta que se haga
        //  _____________
        //if (st[4] != "s") {
        //    if (st[4] == "h") {
        //        $help.hide();
        //    } else {
        //        if (st[4] == "e") {
        //            $help.button({
        //                disabled: false
        //            });
        //        } else {
        //            $help.button({
        //                disabled: true
        //            });
        //        }
        //    }
        //}

        if (st[5] != "s") {
            if (st[5] == "h") {
                $close.hide();
            } else {
                if (st[5] == "e") {
                    $close.button({
                        disabled: false
                    });
                } else {
                    $close.button({
                        disabled: true
                    });
                }
            }
        }
    },

    ClearSearch: function(fid) {
        var $childs = $("#form_" + fid + " #buscar .search");
        $childs.each(function() {
            $(this).val("");
        });
    },

    SearchAutocomplete: function(fid) {
        $(function() {
            var cache = { }, lastXhr;
            $("#form_" + fid + " #buscar #Nombre").autocomplete({
                minLength: 2,
                source: function getData(request, response) {
                    $.RefreshGrid({ fid: fid });
                }
            });
        });
    },

    RefreshGrid: function() {
        // fid: Nombre de la clase.
        // target: Elemento del DOM donde se mostrará el resultado.
        // page: Número de la página donde está actualmente el paginado.
        // filtered: Bool que indica si hay aplicado un filtro o no. Por defecto es true.
        // action: Acción del controlador.

        var args = arguments[0] || { };
        var page = (args.page == "undefined" || args.page == null) ? 1 : args.page;
        var filtered = (args.filtered == "undefined" || args.filtered == null) ? true : args.filtered;
        var fid = args.fid;
        var target = (args.target == "undefined" || args.target == null) ? "#form_" + fid + " #grid" : args.target;
        var action = (args.action == "undefined" || args.action == null) ? fid + "/Search" : args.action;

        //alert(fid);

        var $btn_filter = $("#form_" + fid + " #toolbar #filter");

        var $searchFor = $("#form_" + fid + " #buscar .search");

        var sh = false;
        var count = 0;
        $searchFor.each(function() {
            if ($(this).val() == "") {
                count++;
            }
        });
        if ($searchFor.length == count) {
            sh = true;
        }

        if (!filtered || sh) {
            $.ClearSearch(fid);
            $btn_filter.hide();
        } else {
            $btn_filter.show();
        }

        var filter = "{ ";

        $searchFor.each(function() {
            var $this = $(this);
            filter += $this.attr("id") + ":'" + $this.val() + "', ";
        });

        var $searchForMore = $("#form_" + fid + " #more .filter");
        $searchForMore.each(function() {
            var $this = $(this);
            var valor = $this.val();
            if (valor == null || valor == "undefined" || valor == "foo") {
                valor = "";
            }
            filter += $this.attr("id") + ":'" + valor + "', ";
        });

        filter = filter.substring(0, filter.length - 2) + " }";

        //alert(filter);
        //alert(action);

        var filter = filter;
        $.ajax({
            url: $.HostUrl() + action,
            data: { filtro: filter, page: page, itemsPerPage: $("#footer_" + fid + " #itemsPerPage").val() },
            type: 'GET',
            dataType: 'html',
            cache: false,
            success: function(response) {
                $(target).html(response);
                //$("#ordIcon-nombre").addClass("ui-icon ui-icon-triangle-1-n");
            },
            error: function(xhr, status) {
                alert(xhr.responseText);
            },
            complete: function(xhr, status) {
            }
        });

    },

    StartToolBar: function() {
        // page: Pagina actual del paginado.
        // actionEdit: Accion del controlador para editar el elemento.
        // actionDelete: Accion del controlador encargada de eliminar los elementos.
        // fid: Nombre de la clase, a través de la cual
        //      se formara el nombre del elemento dentro del cual se creara el dialog.
        // titleDialogDelete: Titulo del dialog de eliminar.
        // textDialogDeleteConfirm: Texto del dialogo de confirmacion de eliminacion.
        // toolbarButtonState: Estado de los botones del toolbar. Orden: filtro, buscar, plus, delete, help, close
        // json: Metodo devuelve Json

        var args = arguments[0] || { };
        var page = (args.page == null || args.page == null) ? 1 : args.page;
        var fid = args.fid;
        var actionEdit = (args.actionEdit == "undefined" || args.actionEdit == null) ? fid + "/Edit" : args.actionEdit;
        var json = (args.json == "undefined" || args.json == null) ? "html" : args.json;
        var actionDelete = (args.actionDelete == "undefined" || args.actionDelete == null) ? fid + "/DeleteRows" : args.actionDelete;
        
        var titleDialogDelete = (args.titleDialogDelete == "undefined" || args.titleDialogDelete == null) ? "Información" : args.titleDialogDelete;
        var titleDialogEditClass = (args.titleDialogEditClass == "undefined" || args.titleDialogEditClass == null || args.titleDialogEditClass == "") ? fid : args.titleDialogEditClass;
        var textDialogDeleteConfirm = (args.textDialogDeleteConfirm == "undefined" || args.textDialogDeleteConfirm == null) ? "Se eliminarán los elementos de forma permanente. ¿Está seguro de eliminar?" : args.textDialogDeleteConfirm;
        var toolbarButtonState = (args.toolbarButtonState == "undefined" || args.toolbarButtonState == null) ? "heedee" : args.toolbarButtonState;

        //autocomplete de la busqueda
        $.SearchAutocomplete(fid);

        //botones del toolbar y la busqueda
        var idDialogEdit = "edit_" + fid;

        var $btn_plus = $("#form_" + fid + " #toolbar #plus").button({
            text: false,
            icons: {
                primary: "ui-icon-plus"
            }
        });
        $btn_plus.button().click(function (e) {

            $.Edit({ fid: fid, idDialogEdit: idDialogEdit, actionEdit: actionEdit, titleEditClass: titleDialogEditClass });
            e.preventDefault();
        });

        var $btn_delete = $("#form_" + fid + " #toolbar #delete").button({
            text: false,
            icons: {
                primary: "ui-icon-minus"
            }
        });
        $btn_delete.button().click(function(e) {
            $.DeleteRow({ fid: fid, titleDialogDelete: titleDialogDelete, textDialogDeleteConfirm: textDialogDeleteConfirm, actionDelete: actionDelete });
            e.preventDefault();
        });

        var $btn_close = $("#form_" + fid + " #toolbar #close").button({
            text: false,
            icons: {
                primary: "ui-icon-close"
            }
        });

        $btn_close.button().click(function(e) {
            $.CloseViews();
            e.preventDefault();
        });

        var $btn_help = $("#form_" + fid + " #toolbar #help").button({
            text: false,
            icons: {
                primary: "ui-icon-help"
            }
        });

        var $btn_search = $("#form_" + fid + " #toolbar #search").button({
            text: false,
            icons: {
                primary: "ui-icon-search"
            }
        });
        $btn_search.button().click(function(e) {
            $("#form_" + fid + " #buscar").toggle("drop");
            e.preventDefault();
        });


        var $btn_closeSearch = $("#form_" + fid + " #buscar #closeSearch").button({
            text: false,
            icons: {
                primary: "ui-icon-close"
            }
        });
        $btn_closeSearch.button().click(function(e) {
            $("#form_" + fid + " #buscar").hide("drop");
            e.preventDefault();
        });

        var $btn_searchButton = $("#form_" + fid + " #buscar #buscarButon").button({
            text: true
        });
        $btn_searchButton.button().click(function() {
            $.RefreshGrid({ fid: fid });
        });

        var $btn_limpiarButton = $("#form_" + fid + " #buscar #limpiarButon").button({
            text: true
        });
        $btn_limpiarButton.button().click(function() {
            $.ClearSearch(fid);
        });

        var $btn_filter = $("#form_" + fid + " #toolbar #filter");
        $btn_filter.Laibutton("lai-icon-filter");

        $btn_filter.click(function() {
            $.ClearSearch(fid);
            $.RefreshGrid({ fid: fid });
        });

        $btn_filter.mouseenter(function() {
            if ($("#form_" + fid + " #buscar").is(':hidden')) {
                $("#form_" + fid + " #buscar").show("drop");
            }
        });


        $("#form_" + fid + " #buscar").hide();

        $("#form_" + fid + " #delall").click(function() {
            if ($(this).attr("checked") == "checked") {
                $("#form_" + fid + " .chk").attr("checked", "true");
                $.GetChkboxsMarcados();
            } else {
                $("#form_" + fid + " .chk").removeAttr("checked");
                $.GetChkboxsMarcados();
            }
        });

        $("#form_" + fid + " .chk").click(function() {
            $.GetChkboxsMarcados();
        });

        $.ChangeStateButtonToolBar(fid, toolbarButtonState);
        var id = $("#EditItemId").val();
        if (id != 0) {
            $.Edit({ fid: fid, idDialogEdit: idDialogEdit, actionEdit: actionEdit, titleEditClass: titleDialogEditClass });
        }
    },

    StartGetElements: function() {
        var args = arguments[0] || { };
        var fid = args.fid;
        var titleDialogEditClass = (args.titleDialogEditClass == "undefined" || args.titleDialogEditClass == null) ? fid : args.titleDialogEditClass;

        //opciones del menu contextual   
        $("#form_" + fid + " .DeleteMenu").click(function() {
            $(".cmenu").hide();
            $.DeleteRow({ fid: fid, ids: this.value + "_" });
            return false;
        });

        $("#form_" + fid + " .EditarMenu").click(function() {
            $(".cmenu").hide();
            var element = this.value;
            var page = $("#form_" + fid + " #currentPage").val();
            $.Edit({ fid: fid, page: page, titleEditClass: titleDialogEditClass, elemtId: element });
            return false;
        });

        $(".AyudaMenu").click(function() {
            alert("AYUDA");
            $(".cmenu").hide();
            return false;
        });

        $("#form_" + fid + " table:#list tr:odd").removeClass("tableimpar");
        $("#form_" + fid + " table:#list tr:odd").addClass("tableimpar");

        $("#form_" + fid + " .orden").click(function() {
            var $orderHidden = $("#form_" + fid + " #order");
            var des = $orderHidden.val();
            var order = $(this).html();
            $.OrderGrid({ fid: fid, order: order, descending: des });
        });

        $("#form_" + fid + " .chk").click(function() {
            $.GetChkboxsMarcados(fid);
        });

        $("#form_" + fid + " #delall").click(function() {
            if ($(this).attr("checked") == "checked") {
                $("#form_" + fid + " .chk").attr("checked", "true");
                $.GetChkboxsMarcados(fid);
            } else {
                $("#form_" + fid + " .chk").removeAttr("checked");
                $.GetChkboxsMarcados(fid);
            }
        });

        $(document).click(function(e) {
            if (e.target.parentNode.id != "menuButton") {
                $(".cmenu").hide();
            }
        });

        $(document).keydown(function(e) {
            if (e.keyCode == 27) {
                $(".cmenu").hide();
            }
        });
    },

    OrderGrid: function() {
        // order: Ordenar por..
        // fid: Nombre de la clase
        // descending: Ascendente o descendente.
        // action: Acción del controlador que ordena.
        // target: Colocar el resultado del ordenamiento en...

        var args = arguments[0] || { };
        var fid = args.fid;
        var order = args.order;
        var action = (args.action == "undefined" || args.action == null) ? fid + "/Order" : args.action;
        var target = (args.target == "undefined" || args.target == null) ? "#form_" + fid + " #grid" : args.target;
        var descending = (args.descending == "undefined" || args.descending == null) ? 0 : args.descending;

        $.ajax({
            url: $.HostUrl() + action,
            data: { order: order, descending: descending },
            type: 'GET',
            dataType: 'html',
            cache: false,
            success: function(response) {
                $(target).html(response);
                $.ChangeOrder(order, fid);
            },
            error: function(xhr, status) {
                alert("Error");
            },
            complete: function(xhr, status) {
            }
        });
    },

    ChangeOrder: function(orderBy, fid) {
        $("#form_" + fid + " th span").each(function() {
            $(this).removeClass("ui-icon ui-icon-triangle-1-n ui-icon-triangle-1-s");
        });

        $orderHidden = $("#form_" + fid + " #order");

        if ($orderHidden.val() == "1") {
            $("#form_" + fid + " #ordIcon-" + orderBy).addClass("ui-icon ui-icon-triangle-1-s");
            $orderHidden.val("0");
        } else {
            $("#form_" + fid + " #ordIcon-" + orderBy).addClass("ui-icon ui-icon-triangle-1-n");
            $orderHidden.val("1");
        }
    },

    GetChkboxsMarcados: function(fid) {
        var marcados = $("#form_" + fid + " .chk:checked");
        var ids = "";
        $(marcados).each(function(i) {
            ids += marcados[i]["value"] + "_";
        });
        if (ids == "")
            $.ChangeStateButtonToolBar(fid, "sssdss");
        else
            $.ChangeStateButtonToolBar(fid, "sssess");
        $("#form_" + fid + " #idMarcados").val(ids);
    },

    CreateDialogModal: function (title, dimensiones, fnClose, fnOpen) {

      
        

        var dim = dimensiones;
        
        if (dim != null && dim.reset == true) {
            window.scrollTo(0, 0);
        }

        var alto = 'auto';
        if (dim != null && dim.height !=null) {
            alto = dim.height;
        }
        
        var arriba = 'top';
        if (dim != null && dim.top != null) {
            arriba = dim.top;
        }
        
        var izq = 'center';
        if (dim != null && dim.left != null) {
            izq = dim.left;
        }
      
        $.appGlobales.dialogoscreados = parseInt($.appGlobales.dialogoscreados) + 1;
        var fid = $.appGlobales.dialogoscreados;

        var $tar = $("#areaDialogos #aqui").before("<div id='dialog_" + fid + "' class='ui-widget-content ui-helper-clearfix'></div>");
        var $dialo = $("#areaDialogos #dialog_" + fid);
        $dialo.dialog({
            autoOpen: false,
            height: alto,
            width: 'auto',
            position: [izq, arriba],
            modal: true,
            resizable: false,
            show: 'slide',
            cache: false,
            buttons: {},            
            title: title,
            open: function() {
                if (fnOpen != null) {
                    var res = fnOpen();
                }
            },
            close: function() {

                if (fnClose != null) {
                    var res = fnClose();
                }
                if ($.appGlobales.stack.length > 0) {
                    $.appGlobales.stack.pop().remove();

                }
                $.appGlobales.clearIntervals();
                $.appGlobales.HideMenuFlotante();
            }
        });
        $.appGlobales.stack.push($dialo);
        return $dialo;
    },
});

jQuery.fn.DropDownSelection = function(texto) {
    this.append('<option value="foo" selected="selected">Seleccionar ' + texto + '</option>');

    this.change(function() {
        $("#" + this.id + " option[value='foo']").remove();
    });
};

jQuery.fn.Laibutton = function (icon) {
    var obje = $(this);
    obje.addClass("lai-button ui-button ui-widget lai-state-default ui-corner-all lai-button-icon-only")
        .html("<span class='lai-button-icon-primary lai-icon " + icon + "'></span>");

    this.mouseenter(function () {
        $(this).addClass("lai-state-hover");
    });
    this.mouseleave(function () {
        $(this).removeClass("lai-state-hover");
    });
};

jQuery.fn.SpanishDatePicker = function() {
    // id: Id del elemento que se convertirá en datepiker
    var dates = this.datepicker({
        showAnim: 'slide',
        closeText: 'Cerrar',
        prevText: '&#x3c;Ant',
        nextText: 'Sig&#x3e;',
        currentText: 'Hoy',
        monthNames: ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
            'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'],
        monthNamesShort: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun',
            'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'],
        dayNames: ['Domingo', 'Lunes', 'Martes', 'Mi&eacute;rcoles', 'Jueves', 'Viernes', 'S&aacute;bado'],
        dayNamesShort: ['Dom', 'Lun', 'Mar', 'Mi&eacute;', 'Juv', 'Vie', 'S&aacute;b'],
        dayNamesMin: ['Do', 'Lu', 'Ma', 'Mi', 'Ju', 'Vi', 'S&aacute;'],
        weekHeader: 'Sm',
        dateFormat: 'dd/mm/yy',
        firstDay: 1,
        isRTL: false,
        showMonthAfterYear: false,
        yearSuffix: '',
        defaultDate: "+1w",
        changeMonth: true,
        numberOfMonths: 1,
        onSelect: function(selectedDate) {
            var option = this.id == "from" ? "minDate" : "maxDate",
                instance = $(this).data("datepicker"),
                date = $.datepicker.parseDate(
                    instance.settings.dateFormat ||
                        $.datepicker._defaults.dateFormat,
                    selectedDate, instance.settings);
            dates.not(this).datepicker("option", option, date);
        }
    });
};

$(document).ready(function () {
    

});


     