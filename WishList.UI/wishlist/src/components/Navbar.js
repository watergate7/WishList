import React, { Component } from 'react';
import { Nav, Navbar, NavItem } from 'react-bootstrap';
import { NavLink } from 'react-router-dom';

class AppNavbar extends Component {
    render() {
        const navbar = (<Navbar inverse collapseOnSelect>
            <Navbar.Header>
                <Navbar.Brand>
                    <a>Wish List</a>
                </Navbar.Brand>
                <Navbar.Toggle />
            </Navbar.Header>
            <Navbar.Collapse>
                <Nav>
                    <NavItem eventKey={1}>
                        <NavLink to="/">Wish Gallery</NavLink>
                    </NavItem>
                    <NavItem eventKey={2}>
                        <NavLink to="/makewish">Make a wish</NavLink>
                    </NavItem>
                </Nav>
            </Navbar.Collapse>
        </Navbar>);

        return navbar;
    }
}

export default AppNavbar;

