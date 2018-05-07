import React, { Component } from 'react';
import { FormGroup, FormControl, ControlLabel, HelpBlock, Grid, Row, Col, Button, Label } from 'react-bootstrap';
const queryString = require('qs');

class MakeWish extends Component {
    constructor(props) {
        super(props);
        this.state = { name: '', type: '', brand: '', no: '', price: undefined, picture: null };
        var parsed = queryString.parse(this.props.location.search.slice(1));
        if (parsed.afterAdd == 1) {
            this.state.afterAdd = "inline";
        }
        else {
            this.state.afterAdd = "none";
        }

        this.handleChange = this.handleChange.bind(this);
    }

    handleChange(event) {
        const name = event.target.id;
        const value = name == "picture" ? event.target.files[0] : event.target.value;
        this.setState({ [name]: value });

        if (this.state.afterAdd == "inline") {
            this.state.afterAdd = "none";
        }
    }

    render() {
        const formInstance = (
            <form name="wishForm" encType="multipart/form-data" action="../api/WishList/Add" method="post">
                <Grid>
                    <Row className="afterAdd">
                        <Col xs={4} md={4}>
                            <Label bsStyle="success" style={{ display: this.state.afterAdd }}>You wish is added successfully!</Label>
                        </Col>
                    </Row>
                    <Row className="show-grid">
                        <Col xs={6} md={4}>
                            <FieldGroup
                                id="name"
                                name="name"
                                type="text"
                                label="Name"
                                value={this.state.name}
                                onChange={this.handleChange}
                                placeholder="Enter Name"
                            />
                        </Col>
                        <Col xs={6} md={4}>
                            <FieldGroup
                                id="type"
                                name="type"
                                label="Type"
                                type="text"
                                value={this.state.type}
                                onChange={this.handleChange}
                                placeholder="Enter Type"
                            />
                        </Col>
                    </Row>
                    <Row className="show-grid">
                        <Col xs={6} md={4}>
                            <FieldGroup
                                id="brand"
                                name="brand"
                                label="Brand"
                                type="text"
                                value={this.state.brand}
                                onChange={this.handleChange}
                                placeholder="Enter Brand"
                            />
                        </Col>
                        <Col xs={6} md={4}>
                            <FieldGroup
                                id="no"
                                name="no"
                                type="text"
                                label="Number"
                                value={this.state.no}
                                onChange={this.handleChange}
                                placeholder="Enter Item No."
                            />
                        </Col>
                        <Col xs={9} md={4}>
                            <Row className="show-grid">
                                <Col xs={8} md={8}>
                                    <FieldGroup
                                        id="price"
                                        name="price"
                                        type="number"
                                        label="Price"
                                        value={this.state.price}
                                        onChange={this.handleChange}
                                        placeholder="Enter Estimated Price"
                                    />
                                </Col>
                                <Col xs={4} md={4}>
                                    <CurrencySelect
                                        id="currency"
                                        name="currency"
                                        componentClass="select"
                                    />
                                </Col>
                            </Row>
                        </Col>
                    </Row>
                    <Row>
                        <Col xs={4} md={4}>
                            <FieldGroup
                                id="picture"
                                name="picture"
                                type="file"
                                label="Picture"
                                help="Upload a picture here."
                                accept="image/gif, image/jpeg, image/png"
                                onChange={this.handleChange}
                            />
                        </Col>
                    </Row>
                    <Row>
                        <Col xs={4} md={4}>
                            <Button type="submit">Submit</Button>
                        </Col>
                    </Row>
                </Grid>
            </form>);

        return formInstance;
    }
}

function FieldGroup({ id, label, help, ...props }) {
    return (
        <FormGroup controlId={id}>
            <ControlLabel>{label}</ControlLabel>
            <FormControl {...props} />
            {help && <HelpBlock>{help}</HelpBlock>}
        </FormGroup>
    );
}

function CurrencySelect({ id, label, help, ...props }) {
    return (
        <FormGroup controlId={id}>
            <ControlLabel>{label}</ControlLabel>
            <FormControl {...props}>
                <option value="CNY">CNY</option>
                <option value="USD">USD</option>
                <option value="EUR">EUR</option>
                <option value="GBP">GBP</option>
                <option value="JPY">JPY</option>
                <option value="AUD">AUD</option>
                <option value="HKD">HKD</option>
                <option value="KRW">KRW</option>
            </FormControl>
            {help && <HelpBlock>{help}</HelpBlock>}
        </FormGroup>
    );
}

export default MakeWish;



